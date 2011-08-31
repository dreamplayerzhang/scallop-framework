/*
 * The Scallop framework (including the binary executables and the source
 * code) is distributed under the terms of the MIT license.
 *  
 * Copyright (c) 2008 Machine Vision Group, Oulu University
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Reflection;
using System.Xml.Linq;
using System.Linq;
using System.Windows.Media.Imaging;

using Scallop.Core;
using Scallop.Core.Events;
using Scallop.Core.Sensor;

[assembly: System.CLSCompliant(true)]

namespace Scallop.Sensor.Axis
{
   /// <summary>
   /// Axis IP camera class.
   /// </summary>
   public class AxisCamera : IScallopSensor, IDisposable
   {
      /* Class fields */

      //private AxisParameters camParams;
      private AxisCameraConfigType cameraConfig;
      private ScallopSensorState myState = ScallopSensorState.Undefined;
      private BackgroundWorker frameHandlerThread;
      private bool registered = false;
      private bool streaming = false;
      private bool _disposed = false;

      private XmlSchema configSchema;

      /// <summary>
      /// Constructor. Sets up the configuration schema.
      /// </summary>
      /// <exception cref="ApplicationException">Thrown when there is an error with getting the schema from the assembly.</exception>
      public AxisCamera()
      {
         try
         {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Scallop.Sensor.Axis.AxisCameraConfig.xsd");
            XmlSerializer serializer = new XmlSerializer(typeof(XmlSchema));
            this.configSchema = (XmlSchema)(serializer.Deserialize(stream));

            this.myState = ScallopSensorState.Idle;
         }
         catch (Exception ex)
         {
            throw new ScallopException("Reading Axis configuration schema failed.", ex);
         }
      }

      /// <summary>
      /// Object destructor
      /// </summary>
      ~AxisCamera()
      {
         Dispose(false);
      }

      #region IDisposable Members

      /// <summary>
      /// Frees the resources used by the object.
      /// </summary>
      public void Dispose()
      {
         Dispose(true);

         // Use SupressFinalize in case a subclass
         // of this type implements a finalizer.
         GC.SuppressFinalize(this);
      }

      /// <summary>
      /// Frees the resources used by the object
      /// </summary>
      /// <param name="disposing">The parameter tells if the Dispose is called directly</param>
      protected virtual void Dispose(bool disposing)
      {
         if (!_disposed)
         {
            if (disposing)
            {
               if (this.frameHandlerThread != null)
               {
                  this.Stop();
                  this.frameHandlerThread.Dispose();
               }
            }
            this.frameHandlerThread = null;
         }
      }

      #endregion

      #region IScallopSensor members


      #region Old interface methods
      ///// <summary>
      ///// Registers a node with a sensor.
      ///// </summary>
      ///// <param name="configDoc">The configuration XML document.</param>
      ///// <param name="selectConfig">String identifying the configuration item to use.</param>
      //public void Register(XmlDocument configDoc, string selectConfig)
      //{
      //   // validate the xml
      //   configDoc.Schemas.Add(this.configSchema);
      //   configDoc.Validate(null);

      //   // parse the settings from the xml


      //   this.camParams = AxisParameters.ParseConfig(configDoc, selectConfig);
      //   if (this.camParams == null)
      //   {
      //      throw new ApplicationException("StatusChanged parsing config");
      //   }

      //   this.registered = true;
      //}

      #endregion
      /// <summary>
      /// Registers a node with a sensor.
      /// </summary>
      /// <param name="config">The configuration XML document.</param>
      /// <param name="selectConfig">String identifying the configuration item to use.</param>
      public void Register(XDocument config, string selectConfig)
      {
         // validate the xml
         XmlSchemaSet schemas = new XmlSchemaSet();
         schemas.Add(this.configSchema);
         config.Validate(schemas, null);

         //this.camParams = AxisParameters.ParseConfig(configDoc, selectConfig);
         this.cameraConfig = AxisCamera.GetConfig(config, selectConfig);

         if (this.cameraConfig == null)
         {
            throw new ScallopException("Error when tryin to parse config");
         }


         this.registered = true;
      }

      private static AxisCameraConfigType GetConfig(XDocument configDoc, string selectConfig)
      {
         AxisCameraConfigType ctSelected = null;

         XElement root = configDoc.Root;

         XmlSerializer xmlConfigReader = new XmlSerializer(typeof(AxisCameraConfigSet));

         AxisCameraConfigSet configSet = (AxisCameraConfigSet)xmlConfigReader.Deserialize(root.CreateReader());

         var config = (from ct in configSet.Items
                       where ct.ConfigName == selectConfig
                       select ct).FirstOrDefault();

         ctSelected = config as AxisCameraConfigType;

         if (ctSelected == null)
         {
            var configDefault = (from ct in configSet.Items
                                 where ct.ConfigName == configSet.DefaultConfig
                                 select ct).FirstOrDefault();

            ctSelected = configDefault as AxisCameraConfigType;
         }

         return ctSelected;
      }

      /// <summary>
      /// Unregisters a node from a sensor.
      /// </summary>
      public void Unregister()
      {
         this.Stop();
         this.registered = false;
      }

      /// <summary>
      /// Starts receiving sensor data.
      /// </summary>
      /// <exception cref="ApplicationException">Thrown if sensor has not been
      /// registered prior to calling Start().</exception>
      public void Start()
      {
         if (!this.registered)
            throw new ScallopException("Sensor not registered!");

         if (this.streaming)
            return;


         if (frameHandlerThread != null)
            frameHandlerThread.Dispose();
         frameHandlerThread = new BackgroundWorker();
         frameHandlerThread.WorkerSupportsCancellation = true;
         frameHandlerThread.DoWork += this.getFrames;
         frameHandlerThread.RunWorkerCompleted += this.getFrames_completed;
         frameHandlerThread.RunWorkerAsync();
      }


      /// <summary>
      /// Stops receiving sensor data.
      /// </summary>
      public void Stop()
      {
         if (this.frameHandlerThread != null)
            this.frameHandlerThread.CancelAsync();

         this.myState = ScallopSensorState.Idle;
      }


      /// <summary>
      /// Pans the sensor.
      /// </summary>
      /// <param name="absolute">True if absolute pan value, 
      /// false if relative.</param>
      /// <param name="xCoordinate">Degrees to pan on the horizontal axis.</param>
      /// <param name="yCoordinate">Degrees to pan on the vertical axis.</param>
      public void PanTilt(bool absolute, int xCoordinate, int yCoordinate)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Zooms the sensor.
      /// </summary>
      /// <param name="absolute">True if absolute zoom value, 
      /// false if relative.</param>
      /// <param name="zoomValue">The zoom value.</param>
      public void Zoom(bool absolute, int zoomValue)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Current version
      /// </summary>
      public string Version
      {
         get
         {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
         }
      }

      /// <summary>
      /// Gets the configuration XML schema associated with an Axis camera.
      /// </summary>
      public XmlSchema ConfigSchema
      {
         get
         {
            return this.configSchema;
         }
      }

      /// <summary>
      /// Gets the sensor state.
      /// </summary>
      public ScallopSensorState State
      {
         get { return this.myState; }
      }

      /// <summary>
      /// An event indicating the sensor status has changed. This could mean it
      /// has become active, encountered an error condition, etc.
      /// </summary>
      public event EventHandler<ScallopSensorStatusChangedEventArgs> StatusChanged;

      /// <summary>
      /// Fired when a new frame is available. The image data is passed in the
      /// event arguments.
      /// </summary>
      public event EventHandler<ScallopSensorDataEventArgs> Data;

      /// <summary>
      /// Fired when the camera module wants to inform the user of something.
      /// </summary>
      public event EventHandler<ScallopInfoEventArgs> Info;

      #endregion

      private void doStatusChanged(object sender, ScallopSensorStatusChangedEventArgs e)
      {
         ScallopSensorState oldState = this.myState;
         this.myState = e.NewState;
         if (this.StatusChanged != null)
            this.StatusChanged(sender, e);
      }

      private void doError(object sender, ScallopSensorStatusChangedEventArgs e)
      {
         if (this.StatusChanged != null)
            this.StatusChanged(sender, e);

         myState = ScallopSensorState.Error;
      }

      private void doClosed(object sender, EventArgs e)
      {
         if (this.StatusChanged != null)
         {
            this.StatusChanged(this, new ScallopSensorStatusChangedEventArgs(myState, ScallopSensorState.Idle));
         }
         myState = ScallopSensorState.Idle;
      }

      private void doOpened(object sender, EventArgs e)
      {
         if (this.StatusChanged != null)
         {
            this.StatusChanged(this, new ScallopSensorStatusChangedEventArgs(myState, ScallopSensorState.Active));
         }
         myState = ScallopSensorState.Active;
      }

      private void doInfo(object sender, ScallopInfoEventArgs e)
      {
         if (this.Info != null)
            this.Info(sender, e);
      }

      private void doData(object sender, ScallopSensorDataEventArgs e)
      {
         if (this.Data != null)
            this.Data(sender, e);
      }



      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void getFrames_completed(object sender, RunWorkerCompletedEventArgs e)
      {

         if (e.Cancelled) // CancelAsync called
         {
            this.streaming = false;
            this.doClosed(this, EventArgs.Empty);
            return;
         }
         else if (e.Error != null) // Exception occured
         {
            this.doError(this, new ScallopSensorStatusChangedEventArgs(myState, ScallopSensorState.Error, "", e.Error));
            this.myState = ScallopSensorState.Error;
            this.doClosed(this, EventArgs.Empty);
            this.streaming = false;
            this.doInfo(this, new ScallopInfoEventArgs("Retrying in 5 seconds..."));
            Thread.Sleep(5000);
            this.Start();

            return;
         }
         else
         {
            System.Diagnostics.Debug.Assert(false, /* we should never get here! */
              "An impossible event happened at getFrames_completed");
         }
      }

      /// <summary>
      /// Parse the MJPG stream. This should only return in the case of an 
      /// exception accuring.
      /// </summary>
      /// <exception cref="System.ApplicationException">Thrown when
      /// there is a problem with the handling of the stream. See the
      /// string description for more information.</exception>
      //private void getFrames()
      private void getFrames(object sender, DoWorkEventArgs ev)
      {
         BackgroundWorker bw = sender as BackgroundWorker;

         string strHeader = "--myboundary\r\nContent-Type: image/jpeg\r\nContent-Length: ";

         byte[] headerBuffer = new byte[strHeader.Length];
         string strHeaderBuffer = "";

         int bytesRead = 0;

         // Set the URL to request
         //string strRequest = this.camParams.MjpgParameterString();
         string strRequest = AxisParameters.MjpgParameterString(this.cameraConfig);
         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strRequest);

         // set the username and password
         if (this.cameraConfig.User != null && this.cameraConfig.Password != null)
         {
            request.Credentials = new NetworkCredential(
                                      this.cameraConfig.User,
                                      this.cameraConfig.Password
                                      );
         }

         this.doInfo(this, new ScallopInfoEventArgs("Trying Axis camera with URL " + request.RequestUri.ToString()));

         HttpWebResponse streamResponse = (HttpWebResponse)request.GetResponse();
         this.doInfo(this, new ScallopInfoEventArgs("Server responded with : " + streamResponse.StatusDescription));
         
         using (BufferedStream mjpgStream = new BufferedStream(streamResponse.GetResponseStream()))
         {
            this.streaming = true;
            this.doOpened(this, EventArgs.Empty);
            while (true)
            {
               int contentLen = 0;
               string aLine = "";
               if (bw.CancellationPending == true)
               {
                  mjpgStream.Close();
                  ev.Cancel = true;
                  return;
               }

               /*
               HTTP/1.0 200 OK\r\n
               Content-Type: multipart/x-mixed-replace;boundary=myboundary\r\n
               \r\n
               --myboundary\r\n
               Content-Type: image/jpeg\r\n
               Content-Length: 15656\r\n
               \r\n
               <JPEG image data>\r\n
               --myboundary\r\n
               Content-Type: image/jpeg\r\n
               Content-Length: 14978\r\n
               \r\n
               <JPEG image data>\r\n
               --myboundary\r\n
               Content-Type: image/jpeg\r\n
               Content-Length: 15136\r\n
               \r\n
               <JPEG image data>\r\n
               --myboundary\r\n
                .
                .
                .*/

              bytesRead = 0;

               // Read --myboundary and Content-Type
              while ((bytesRead += mjpgStream.Read(headerBuffer,
                                    bytesRead,
                                    headerBuffer.Length - bytesRead)) < headerBuffer.Length)
               {
                  // No op
               }

               strHeaderBuffer = System.Text.Encoding.UTF8.GetString(headerBuffer);

               if (!strHeaderBuffer.Equals(strHeader))
                  throw new ScallopException("MJPEG header not found");

               aLine = readMjpgLine(mjpgStream);
               contentLen = int.Parse(aLine);

               aLine = readMjpgLine(mjpgStream);
               if (!String.IsNullOrEmpty(aLine)) // empty line
                  throw new ScallopException("Blank line not found");

               // buffer for MJPG frame data
               byte[] frameBuffer = new byte[contentLen];
               bytesRead = 0;

               // read up to contentLen of data to frameBuffer
               while ((bytesRead += mjpgStream.Read(frameBuffer,
                                              bytesRead,
                                              contentLen - bytesRead)) < contentLen)
               {
                  // No op
               }

               aLine = readMjpgLine(mjpgStream);

               switch (this.cameraConfig.FrameFormat)
               {
                  case AxisCameraConfigTypeFrameFormat.Jpeg:
                     this.doData(this, new ScallopSensorDataEventArgs(frameBuffer, "New frame"));
                     break;

                  case AxisCameraConfigTypeFrameFormat.SystemDrawingBitmap:
                     using (MemoryStream pixelStream = new MemoryStream(frameBuffer))
                     {
                        Bitmap streamBitmap = new Bitmap(pixelStream);
                        Bitmap bmp = streamBitmap.Clone() as Bitmap;
                        if (bmp != null)
                           this.doData(this, new ScallopSensorDataEventArgs(bmp, "New frame"));
                        streamBitmap.Dispose();
                     }
                     break;

                  case AxisCameraConfigTypeFrameFormat.SystemWindowsMediaImagingBitmapSource:
                     JpegBitmapDecoder decoder = new JpegBitmapDecoder(new MemoryStream(frameBuffer), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                     if (decoder.Frames[0] != null)
                        this.doData(this, new ScallopSensorDataEventArgs(decoder.Frames[0] as BitmapSource, "New frame"));
                     break;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <returns></returns>
      /// <exception cref="IOException">Thrown when end-of-stream is 
      /// encountered</exception>
      /// <exception cref="WebException"></exception>
      private static string readMjpgLine(Stream input)
      {
         List<byte> buf = new List<byte>();
         string strBuffer;
         int temp =0, temp2 = 0;
         int count = 0;

         while (true)
         {
            temp2 = temp;
            temp = input.ReadByte();

            if (temp == -1)
               throw new ScallopException("End of stream encountered.");
            buf.Add((byte)temp);
            count++;
            if (count > 1 && temp2 == '\r' && temp == '\n')
            {
               break;
            }
         }

         strBuffer = System.Text.Encoding.UTF8.GetString((byte[])buf.ToArray(), 0, count - 2);
         buf.Clear();
         return strBuffer;
      }
   }
}
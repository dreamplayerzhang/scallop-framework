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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.IO;

using Scallop.Core.Sensor;
using Scallop.Core.Events;
using System.Drawing;

[assembly: System.CLSCompliant(true)]

namespace Scallop.Sensor.FileSource
{
   /// <summary>
   /// File source class for playing video files
   /// </summary>
   public class ScallopFileSource : IScallopSensor
   {
      ScallopSensorState myState = ScallopSensorState.Idle;
      FileSourceParameters parameters;

      #region IScallopSensor Members

      //public void Register(System.Xml.XmlDocument config, string selectConfig)
      //{
      //  throw new NotImplementedException();
      //}

      /// <summary>
      /// Registers a node with a sensor.
      /// </summary>
      /// <param name="config">The configuration XML document.</param>
      /// <param name="selectConfig">String identifying the configuration item to use.</param>
      public void Register(System.Xml.Linq.XDocument config, string selectConfig)
      {
         parameters = FileSourceParameters.ParseConfig(config, selectConfig);
         return;
      }

      /// <summary>
      /// Starts receiving sensor data.
      /// </summary>
      public void Start()
      {
         BackgroundWorker frameThread = new BackgroundWorker();
         frameThread.DoWork += new DoWorkEventHandler(frameThread_DoWork);

         frameThread.RunWorkerAsync();
      }

      void frameThread_DoWork(object sender, DoWorkEventArgs e)
      {
         BackgroundWorker bw = (BackgroundWorker)sender;

         MediaPlayer myPlayer = new MediaPlayer();
         myPlayer.Open(parameters.SourceUri);
         myPlayer.Volume = 0;
         myPlayer.Play();

         while (myPlayer.NaturalVideoWidth < 1)
            System.Threading.Thread.Sleep(100);

         myState = ScallopSensorState.Active;
         this.StatusChanged(this, new ScallopSensorStatusChangedEventArgs(ScallopSensorState.Idle, ScallopSensorState.Active, null, "Online"));

         RenderTargetBitmap rtb = new RenderTargetBitmap(myPlayer.NaturalVideoWidth, myPlayer.NaturalVideoHeight,
                                                           96, 96, PixelFormats.Pbgra32);

         while (true)
         {
            if (bw.CancellationPending == true)
            {
               myPlayer.Stop();
               e.Cancel = true;
               return;
            }

            DrawingVisual dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();

            dc.DrawVideo(myPlayer, new Rect(0, 0, myPlayer.NaturalVideoWidth, myPlayer.NaturalVideoHeight));
            dc.Close();

            rtb.Clear();
            rtb.Render(dv);

            BitmapFrame bmp = BitmapFrame.Create(rtb);

            switch (parameters.FrameFormat)
            {
               case ("System.Drawing.Bitmap"):

                  using (MemoryStream memoryStream = new MemoryStream())
                  {
                     //bitmapEncoder.Frames.Add(BitmapFrame.Create(bmp));
                     PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
                     bitmapEncoder.Frames.Add(bmp);
                     bitmapEncoder.Save(memoryStream);
                     memoryStream.Flush();

                     using (Bitmap bmp2 = Bitmap.FromStream(memoryStream) as Bitmap,
                                   bmp3 = new Bitmap(bmp2.Width,bmp2.Height,System.Drawing.Imaging.PixelFormat.Format24bppRgb))
                     {
                        bmp3.SetResolution(bmp2.HorizontalResolution, bmp2.VerticalResolution);

                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp3))
                        {
                           g.DrawImage(bmp2, 0, 0);
                        }   

                        if (this.Data != null)
                           this.Data(this, new ScallopSensorDataEventArgs((System.Drawing.Bitmap)bmp3.Clone(), "new frame"));
                     }

                  }
                  break;

               case ("System.Windows.Media.Imaging.BitmapSource"):
                  if (this.Data != null)
                     this.Data(this, new ScallopSensorDataEventArgs(bmp as BitmapSource, "new frame"));
                  break;
            }

            if (myPlayer.Position >= myPlayer.NaturalDuration)
            {
               myPlayer.Stop();
               myPlayer.Position = TimeSpan.Zero;
               myPlayer.Play();
            }
         }
      }

      /// <summary>
      /// Stops receiving sensor data.
      /// </summary>
      public void Stop()
      {
         return;
      }

      /// <summary>
      /// Pans the sensor.
      /// </summary>
      /// <param name="absolute">True if absolute pan value, 
      /// false if relative.</param>
      /// <param name="x">Degrees to pan on the horizontal axis.</param>
      /// <param name="y">Degrees to pan on the vertical axis.</param>
      public void PanTilt(bool absolute, int x, int y)
      {
         return;
      }

      /// <summary>
      /// Zooms the sensor.
      /// </summary>
      /// <param name="absolute">True if absolute zoom value, 
      /// false if relative.</param>
      /// <param name="zoomValue">The zoom value.</param>
      public void Zoom(bool absolute, int zoomValue)
      {
         return;
      }

      /// <summary>
      /// Gets the current version of the module
      /// </summary>
      public string Version
      {
         get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
      }

      /// <summary>
      /// Gets the configuration XML schema.
      /// </summary>
      public System.Xml.Schema.XmlSchema ConfigSchema
      {
         get { return null; }
      }

      /// <summary>
      /// Gets the sensor state.
      /// </summary>
      public ScallopSensorState State
      {
         get
         {
            return myState;
         }
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

      #region IDisposable Members

      /// <summary>
      /// Frees the resources used by the object.
      /// </summary>
      public void Dispose()
      {
         // stop player etc.
      }

      #endregion
   }
}

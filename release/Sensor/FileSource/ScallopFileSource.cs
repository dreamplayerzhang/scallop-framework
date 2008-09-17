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
using System.Linq;
using System.Text;

using ScallopCore.Sensor;
using ScallopCore.Events;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Animation;

namespace Scallop.Sensor.FileSource
{
  public class ScallopFileSource : IScallopSensor
  {
    ScallopSensorState myState = ScallopSensorState.Idle;
    FileSourceParameters parameters;

    #region IScallopSensor Members

    public void Register(System.Xml.XmlDocument config, string selectConfig)
    {
      return;
    }

    public void Register(System.Xml.Linq.XDocument config, string selectConfig)
    {
      parameters = FileSourceParameters.ParseConfig(config, selectConfig);
      return;
    }

    public void Start()
    {
      BackgroundWorker frameThread = new BackgroundWorker();
      frameThread.DoWork += new DoWorkEventHandler(frameThread_DoWork);

      frameThread.RunWorkerAsync();
    }

    void frameThread_DoWork(object sender, DoWorkEventArgs e)
    {
      // mms://media.oulu.fi

      MediaPlayer myPlayer = new MediaPlayer();
      //myPlayer.Open(new Uri(@"test.avi", UriKind.Relative));
      //myPlayer.Open(new Uri(@"mms://media.oulu.fi", UriKind.Absolute));
      //myPlayer.Open(new Uri(@"behave.avi", UriKind.Relative));
      //myPlayer.Open(new Uri(@"http://akastreaming.yle.fi/vp/fiyle/no_geo/ondemand.asx?gjmf=areena/1/34/73/1347316_859410", UriKind.Absolute));
      myPlayer.Open(parameters.SourceUri);
      myPlayer.Volume = 0;
      myPlayer.Play();

      
      

     
      while ( myPlayer.NaturalVideoWidth < 1 )
        System.Threading.Thread.Sleep(100);

      myState = ScallopSensorState.Active;
      this.StatusChanged(this, new ScallopSensorStatusChangedEventArgs(ScallopSensorState.Idle, ScallopSensorState.Active, null, "Online"));

      while (true)
      {
        RenderTargetBitmap rtb = new RenderTargetBitmap(myPlayer.NaturalVideoWidth, myPlayer.NaturalVideoHeight,
                                                       96, 96, PixelFormats.Pbgra32);
        DrawingVisual dv = new DrawingVisual();
        DrawingContext dc = dv.RenderOpen();

        dc.DrawVideo(myPlayer, new Rect(0, 0, myPlayer.NaturalVideoWidth, myPlayer.NaturalVideoHeight));
        dc.Close();
        rtb.Render(dv);
        BitmapSource bmp = BitmapFrame.Create(rtb);

        BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
        MemoryStream memoryStream = new MemoryStream();
        bitmapEncoder.Frames.Add(BitmapFrame.Create(bmp));
        bitmapEncoder.Save(memoryStream);
        memoryStream.Flush();

        switch (parameters.FrameFormat)
        {
          case ("System.Drawing.Bitmap"):
            System.Drawing.Bitmap bmp2 = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
            System.Drawing.Bitmap bmp3 = new System.Drawing.Bitmap(bmp2.Size.Width, bmp2.Size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            
            bmp3.SetResolution(bmp2.HorizontalResolution,bmp2.VerticalResolution);
            
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp3);
            g.DrawImage((System.Drawing.Bitmap)bmp2.Clone(), 0, 0);
            g.Dispose();
            

            if ( this.Data != null )
              this.Data(this, new ScallopSensorDataEventArgs((System.Drawing.Bitmap)bmp3.Clone(), "new frame"));
            break;

          case("System.Windows.Media.Imaging.BitmapSource"):
            if (this.Data != null)
              this.Data(this, new ScallopSensorDataEventArgs(bmp, "new frame"));
            break;
        }
        
        

        //bmp3.Dispose();
        //bmp2.Dispose();
       
        //System.Threading.Thread.Sleep(100);
        if (myPlayer.Position >= myPlayer.NaturalDuration)
        {
          myPlayer.Stop();
          myPlayer.Position = TimeSpan.Zero;
          myPlayer.Play();
        }
      }
      
    }



    public void Stop()
    {
      throw new NotImplementedException();
    }

    public void PanTilt(bool absolute, int x, int y)
    {
      throw new NotImplementedException();
    }

    public void Zoom(bool absolute, int zoomValue)
    {
      throw new NotImplementedException();
    }

    public string Version
    {
      get { throw new NotImplementedException(); }
    }

    public System.Xml.Schema.XmlSchema ConfigSchema
    {
      get { throw new NotImplementedException(); }
    }

    public ScallopSensorState State
    {
      get
      {
        return myState;
      }
    }

    public event ScallopSensorStatusChangedHandler StatusChanged;

    public event ScallopSensorDataHandler Data;

    public event ScallopSensorInfoHandler Info;

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      // stop player etc.
    }

    #endregion
  }
}

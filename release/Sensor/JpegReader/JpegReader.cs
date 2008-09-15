using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ScallopCore.Events;
using ScallopCore.Sensor;
using OpenCVDotNet;
using System.ComponentModel;
using System.Xml.Linq;

namespace Scallop.Sensor.JpegReader
{
  public class JpegReader : IScallopSensor
  {
    string baseName;
    string dir;
    string frameFormat;
    int fps = 5;
    int seqNum = 0;
    int maxFrame = 150;
    bool StopRequested = false;
    BackgroundWorker frameReader;

    #region IScallopSensor Members

    

    public System.Xml.Schema.XmlSchema ConfigSchema
    {
      get { throw new NotImplementedException(); }
    }
    
    public event ScallopSensorClosedHandler Closed;
    public event ScallopSensorDataHandler Data;
    public event ScallopSensorStatusChangedHandler StatusChanged;
    public event ScallopSensorInfoHandler Info;
    public event ScallopSensorOpenedHandler Opened;

    public void PanTilt(bool absolute, int x, int y)
    {
      throw new NotImplementedException();
    }

    public void Register(System.Xml.Linq.XDocument configDoc, string selectConfig)
    {
      XElement root = configDoc.Root;
      string defConfig = root.Attribute("DefaultConfig").Value.ToString();
      if (selectConfig == null)
        selectConfig = defConfig;

      IEnumerable<XElement> configs =
        from el in root.Elements(XName.Get("JpegReaderConfig", "Scallop/JpegReaderSchema.xsd"))
        where el.Attribute("ConfigName").Value == selectConfig
        select el;

      if (configs.Count<XElement>() != 1)
        throw new ApplicationException("Ambiguous config name, " + selectConfig);

      XElement activeConfig = configs.First<XElement>();

      this.dir = getParamValue(activeConfig, "Directory", "string");
      this.baseName = getParamValue(activeConfig, "BaseName", "string");
      this.frameFormat = getParamValue(activeConfig, "FrameFormat", "string");
      this.fps = int.Parse(getParamValue(activeConfig, "Framerate", "int"));
    }

    private static string getParamValue(XElement config, string item, string type)
    {
      int tmp;
      XElement target = config.Element(XName.Get(item, "Scallop/JpegReaderSchema.xsd"));
      string value;
      value = (target == null ? null : target.Value);
      switch (type)
      {
        case "int":
          return (int.TryParse(value, out tmp) ? target.Value : "-1");
        case "string":
          return (value);
      }

      return target.Value;
    }

    public void Register(XmlDocument config, string selectConfig)
    {
      throw new NotImplementedException();
    }

    public void Start()
    {
      frameReader = new BackgroundWorker();
      frameReader.WorkerSupportsCancellation = true;
      frameReader.DoWork += this.run;
      frameReader.RunWorkerCompleted += this.run_completed;
      frameReader.RunWorkerAsync();

      
    }

    private void run(object sender, DoWorkEventArgs ev)
    {
      while (!StopRequested)
      {
        DateTime start = DateTime.Now;
        string fileName = this.dir + "\\"+ this.baseName + seqNum.ToString("D8") + ".jpg";

        if (this.Data != null && System.IO.File.Exists(fileName) )
        {
          CVImage cvIm = new CVImage(fileName);
          this.Data(this, new ScallopSensorDataEventArgs(cvIm, fileName));
          cvIm.Release();
          
          if (seqNum++ == maxFrame)
            seqNum = 0;
        }
        

        DateTime stop = DateTime.Now;
        System.Threading.Thread.Sleep((1000/fps) - (start - stop).Milliseconds);
      }
    }

    private void run_completed(object sender, RunWorkerCompletedEventArgs e)
    {

    }


    public ScallopSensorState State
    {
      get { throw new NotImplementedException(); }
    }

    public void Stop()
    {
      throw new NotImplementedException();
    }

    public void Unregister()
    {
      throw new NotImplementedException();
    }

    public string Version
    {
      get { return ""; }
    }

    public void Zoom(bool absolute, int zoomValue)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      throw new NotImplementedException();
    }

    #endregion

  }
}

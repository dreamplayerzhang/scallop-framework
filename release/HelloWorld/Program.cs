using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Scallop.Core;
using Scallop.Core.Events;
using Scallop.Core.Network;
using Scallop.Core.Sensor;

using Scallop.Sensor.FileSource;
using Scallop.Network.PeerChannel;



namespace HelloWorld
{
  class Program
  {
    IScallopSensor SensorInterface;
    string SensorConfigFile = "FileSourceConfig.xml";

    IScallopNetwork NetworkInterface;
    string NetworkConfigFile = "PeerChannelConfig.xml";

    static void Main(string[] args)
    {
      Program myProgram = new Program();
      myProgram.Run();
    }

    private void Run()
    {
      // Get an instance of the sensor class using the assembly and class names.
      SensorInterface = InterfaceFactory.CreateSensorInstance("FileSource", "Scallop.Sensor.FileSource.ScallopFileSource");
      // Configure the sensor interface with parameters from an XML file.
      SensorInterface.Register(XDocument.Load(SensorConfigFile),null);
      // Define the handler functions
      SensorInterface.Data += new ScallopSensorDataHandler(SensorInterface_Data);
      SensorInterface.StatusChanged += new ScallopSensorStatusChangedHandler(SensorInterface_StatusChanged);

      // Get instance of network class.
      NetworkInterface = InterfaceFactory.CreateNetworkInstance("PeerChannel", "Scallop.Network.PeerChannel.ScallopPeerChannel");
      // Define handler functions.
      NetworkInterface.Data += new ScallopNetworkDataHandler(NetworkInterface_Data);
      NetworkInterface.StatusChanged += new ScallopNetworkStatusChangedHandler(NetworkInterface_StatusChanged);

      // Start sensor.
      SensorInterface.Start();
      // Join network.
      System.Xml.XmlDocument NetworkConfig = new System.Xml.XmlDocument();
      NetworkConfig.Load(NetworkConfigFile);
      NetworkInterface.Join(NetworkConfig, null);

      for (; ; )
      {
        // infinite loop
      }

    }


    #region Network handlers

    void NetworkInterface_Data(object sender, ScallopNetworkDataEventArgs e)
    {
      ScallopMessage Message = (ScallopMessage)e.data;
      Console.WriteLine("Message received from " + Message.header.sender + " : " + Message.contents);
    }
    
    void NetworkInterface_StatusChanged(object sender, ScallopNetworkStatusChangedEventArgs e)
    {
      Console.WriteLine("Network status changed to " + e.NewState.ToString());
      /* Handle network status changes
       * ...
       *
       */
    }

    #endregion


    #region Sensor handlers

    void SensorInterface_Data(object sender, ScallopSensorDataEventArgs e)
    {
      Console.WriteLine("Sensor data received");
    }
    
    void SensorInterface_StatusChanged(object sender, ScallopSensorStatusChangedEventArgs e)
    {
      Console.WriteLine("Sensor status changed to " + e.NewState.ToString());
      
      /* Handle sensor status changes
       * ...
       *
       */
    }
    
    #endregion
  }
}

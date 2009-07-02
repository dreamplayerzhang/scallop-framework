using System;
using System.Xml.Linq;

using Scallop.Core;
using Scallop.Core.Events;
using Scallop.Core.Network;
using Scallop.Core.Sensor;

namespace Scallop.HelloWorld
{
   class Program
   {
      IScallopSensor SensorInterface = null;
      string SensorConfigFile = "FileSourceConfig.xml";
      //string SensorConfigFile = "SensorConfig.xml";

      IScallopNetwork NetworkInterface = null;
      string NetworkConfigFile = "PeerChannelConfig.xml";

      static void Main(string[] args)
      {
         Program myProgram = new Program();
         myProgram.Run();
      }

      public Program()
      {

      }

      private void Run()
      {
         // Get an instance of the sensor class using the assembly and class names.
         SensorInterface = InterfaceFactory.CreateSensorInstance("Scallop.Sensor.FileSource.dll");
         //SensorInterface = InterfaceFactory.CreateSensorInstance("Scallop.Sensor.Axis.dll");
         Console.WriteLine("Sensor: {0}\tVersion: {1}", SensorInterface.ToString(), SensorInterface.Version);

         // Configure the sensor interface with parameters from an XML file.
         SensorInterface.Register(XDocument.Load(SensorConfigFile), null);
         // Define the handler functions
         SensorInterface.Data += new EventHandler<ScallopSensorDataEventArgs>(SensorInterface_Data);
         SensorInterface.StatusChanged += new EventHandler<ScallopSensorStatusChangedEventArgs>(SensorInterface_StatusChanged);

         // Get instance of network class.
         NetworkInterface = InterfaceFactory.CreateNetworkInstance("Scallop.Network.PeerChannel.dll");
         Console.WriteLine("Network: {0}\tVersion: {1}", NetworkInterface.ToString(), NetworkInterface.Version);

         // Define handler functions.
         NetworkInterface.Data += new EventHandler<ScallopNetworkDataEventArgs>(NetworkInterface_Data);
         NetworkInterface.StatusChanged += new EventHandler<ScallopNetworkStatusChangedEventArgs>(NetworkInterface_StatusChanged);

         // Start sensor.
         SensorInterface.Start();
         // Join network.
         System.Xml.XmlDocument NetworkConfig = new System.Xml.XmlDocument();
         NetworkConfig.Load(NetworkConfigFile);
         NetworkInterface.Join(NetworkConfig, null);

         for (; ; )
         {
            // infinite loop
            System.Threading.Thread.Sleep(1000);
         }

      }


      #region Network handlers

      void NetworkInterface_Data(object sender, ScallopNetworkDataEventArgs e)
      {
         ScallopMessage Message = (ScallopMessage)e.data;
         Console.WriteLine("Message received from " + Message.Header.Sender + " : " + Message.Contents);
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

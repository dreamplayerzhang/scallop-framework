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
      public static IScallopSensor SensorInterface = null;
      string SensorConfigFile = "SensorConfig.xml";

      public static IScallopNetwork NetworkInterface = null;
      string NetworkConfigFile = "PeerChannelConfig.xml";

      static void Main(string[] args)
      {
         try
         {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            Program myProgram = new Program();
            Console.Title = "Scallop HelloWorld: Press <Ctrl>+<C> to close window";
            myProgram.Run();
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.ToString());
         }
      }

      static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
      {
         if (SensorInterface != null)
            SensorInterface.Dispose();

         if (NetworkInterface != null)
            NetworkInterface.Dispose();

         SensorInterface = null;
         NetworkInterface = null;
      }

      public Program()
      {

      }

      private void Run()
      {
         #region Initialize sensor
         // Get an instance of the sensor class using the assembly and class names.
         //SensorInterface = InterfaceFactory.CreateSensorInstance("Scallop.Sensor.FileSource.dll");
         SensorInterface = InterfaceFactory.CreateSensorInstance("Scallop.Sensor.Axis.dll");

         // Define the handler functions
         SensorInterface.Data += new EventHandler<ScallopSensorDataEventArgs>(SensorInterface_Data);
         SensorInterface.StatusChanged += new EventHandler<ScallopSensorStatusChangedEventArgs>(SensorInterface_StatusChanged);
         SensorInterface.Info += new EventHandler<ScallopInfoEventArgs>(SensorInterface_Info);

         // Configure the sensor interface with parameters from an XML file.
         SensorInterface.Register(XDocument.Load(SensorConfigFile), null);
         #endregion

         #region Initialize network
         // Get instance of network class.
         NetworkInterface = InterfaceFactory.CreateNetworkInstance("Scallop.Network.PeerChannel.dll");

         // Define handler functions.
         NetworkInterface.Data += new EventHandler<ScallopNetworkDataEventArgs>(NetworkInterface_Data);
         NetworkInterface.StatusChanged += new EventHandler<ScallopNetworkStatusChangedEventArgs>(NetworkInterface_StatusChanged);
         NetworkInterface.Info += new EventHandler<ScallopInfoEventArgs>(NetworkInterface_Info);
         #endregion

         // Start sensor.
         SensorInterface.Start();
         // Join network.
         System.Xml.XmlDocument NetworkConfig = new System.Xml.XmlDocument();
         NetworkConfig.Load(NetworkConfigFile);
         NetworkInterface.Join(NetworkConfig, null);

         do
         {
            // infinite loop
            System.Threading.Thread.Sleep(1000);
         } while (true);

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

      void NetworkInterface_Info(object sender, ScallopInfoEventArgs e)
      {
         Console.WriteLine(e.msg);
      }

      #endregion


      #region Sensor handlers

      void SensorInterface_Data(object sender, ScallopSensorDataEventArgs e)
      {
         Console.WriteLine("Sensor data received at " + System.DateTime.Now.ToString("HH:mm:ss.ff"));
      }

      void SensorInterface_StatusChanged(object sender, ScallopSensorStatusChangedEventArgs e)
      {
         Console.WriteLine("Sensor status changed to " + e.NewState.ToString());

         /* Handle sensor status changes
          * ...
          *
          */
      }

      void SensorInterface_Info(object sender, ScallopInfoEventArgs e)
      {
         Console.WriteLine(e.msg);
      }

      #endregion
   }
}

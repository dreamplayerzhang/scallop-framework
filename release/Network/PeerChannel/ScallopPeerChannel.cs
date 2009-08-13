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
using System.Xml;
using System.ServiceModel;
using System.Threading;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;

using Scallop.Core.Events;
using Scallop.Core.Network;

namespace Scallop.Network.PeerChannel
{

   /// <summary>
   /// Class representing a ScallopCore PeerChannel network connection.
   /// </summary>
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
   public class ScallopPeerChannel : IScallopNetwork, IPeerChannel, IDisposable
   {
      const string PEERCLOUDNAME = "scallop";

      // channels
      private IPeerChannelChannel chan;
      private DuplexChannelFactory<IPeerChannelChannel> factory;
      private ScallopNetworkState myState = ScallopNetworkState.Undefined;
      private PeerNode peer;

      // config
      private XmlDocument configDocument;
      private string selectedConfig;
      private XmlSchema configSchema;
      private PeerChannelParameters parameters;

      // node id
      private string id;
      private bool registered = false;
      private DateTime lastNeighbourQuery = DateTime.Now;

      private int msgCountRX;
      private int msgCountTX;
      private long msgSizeRX;
      private long msgSizeTX;
      private int hopsSumRX;

      private List<string> neighbours = new List<string>();
      private List<string> oldNeighbours = new List<string>();
      private string lastNeighbourId = "SDSDFSDFSDFSDFSDFDGHSRTHDRT";

      /// <summary>
      /// Constructor.
      /// </summary>
      public ScallopPeerChannel()
      {
         try
         {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Scallop.Network.PeerChannel.PeerChannelConfig.xsd");
            XmlSerializer serializer = new XmlSerializer(typeof(XmlSchema));
            this.configSchema = (XmlSchema)(serializer.Deserialize(stream));
         }
         catch
         {
            throw new ApplicationException("Assembly error");
         }
      }

      /// <summary>
      /// Frees resources.
      /// </summary>
      public void Dispose()
      {
         if (this.chan != null)
            this.Leave();
      }

      /// <summary>
      /// Sends a message to all recipients, broadcast.
      /// </summary>
      /// <param name="message">The message object.</param>
      public void SendMessage(string message)
      {
         ScallopMessage msg = new ScallopMessage();
         msg.Contents = message;
         msg.Header.Sender = this.id;
         msg.HopCount = int.MaxValue;
         this._sendMessage(msg);
      }

      ///// <summary>
      ///// 
      ///// </summary>
      ///// <param name="message"></param>
      ///// <param name="nodeid"></param>
      //public void SendMessage(string message, string nodeid)
      //{
      //  ScallopMessage msg = new ScallopMessage();
      //  msg.contents = message;
      //  string[] recvs = { nodeid };
      //  msg.receivers = recvs;

      //  this._sendMessage(msg);
      //}

      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      /// <param name="nodeids"></param>
      public void SendMessage(string message, params string[] nodeids)
      {
         ScallopMessage msg = new ScallopMessage();
         msg.Contents = message;
         msg.Header.Receivers = nodeids;

         this._sendMessage(msg);
      }

      /// <summary>
      /// Sends a message to all reciepients within the specified number of hops.
      /// </summary>
      /// <param name="message">The message.</param>
      /// <param name="reach">Hopcount.</param>
      public void SendMessage(string message, int reach)
      {
         ScallopMessage msg = new ScallopMessage();
         msg.Contents = message;
         msg.HopCount = reach;

         this._sendMessage(msg);
      }

      private void _sendMessage(ScallopMessage msg)
      {
         try
         {
            if (this.chan != null && this.peer.IsOnline)
            {
               if (msg.Header.Sender == null)
                  msg.Header.Sender = this.id;

               if (!msg.Header.InternalMessage)
               {
                  msgCountTX++;
                  msgSizeTX += (long)msg.Contents.Length;
               }

               msg.Header.OrigHopcount = msg.HopCount;

               chan.PCSend(msg);
            }
         }
         catch (Exception e)
         {
            this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Error, e, "Error sending message"));
            //error_handler(this, new UnhandledExceptionEventArgs(e, false));
         }
      }



      /// <summary>
      /// Joins a PeerChannel.
      /// </summary>
      /// <param name="configDoc">Configuration XML document.</param>
      /// <param name="selectConfig">String identifying config item to use.</param>
      public void Join(XmlDocument configDoc, string selectConfig)
      {
         if (NetPeerTcpBinding.IsPnrpAvailable == false)
         {
            this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Error, null, "PNRP is not installed and configured!"));
            return;
         }

         AppDomain.CurrentDomain.UnhandledException += this.error_handler;

         try
         {
            // parse config
            if (configDoc == null)
            {
               this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Error, null, "Config is null"));
               return;
            }

            configDoc.Schemas.Add(this.configSchema);
            configDoc.Validate(null);

            this.configDocument = configDoc;
            this.selectedConfig = selectConfig;

            this.parameters = PeerChannelParameters.ParseConfig(configDoc, selectConfig);
            if (this.parameters == null)
            {
               throw new ApplicationException("StatusChanged parsing config");
            }

            this.id = (parameters.NodeId != null) ? parameters.NodeId : System.Guid.NewGuid().ToString();
            this.registerPeerChannel(this.id);

            BackgroundWorker queryThread = new BackgroundWorker();
            queryThread.WorkerSupportsCancellation = true;
            queryThread.DoWork += new DoWorkEventHandler(queryThread_DoWork);
            queryThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(queryThread_RunWorkerCompleted);
            queryThread.RunWorkerAsync();

            return;
         }
         catch (Exception e)
         {
            error_handler(this, new UnhandledExceptionEventArgs(e, false));
            return;
         }
      }

      void queryThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {

      }

      void queryThread_DoWork(object sender, DoWorkEventArgs e)
      {
         for (; ; )
         {
            this.oldNeighbours = new List<string>(this.neighbours); // copy neighbours to oldneighbours
            this.neighbours.Clear();
            ScallopMessage msg = new ScallopMessage();
            this.lastNeighbourId = Guid.NewGuid().ToString();
            msg.Contents = "QUERY" + lastNeighbourId;
            msg.Header.Sender = this.id;
            msg.Header.InternalMessage = true;
            msg.HopCount = 1;
            msg.Header.OrigHopcount = 1;
            this._sendMessage(msg);
            Thread.Sleep(1000 * parameters.NeighborQueryRate);
         }
      }


      /// <summary>
      /// Leaves the PeerChannel.
      /// </summary>
      public void Leave()
      {
         if (this.State == ScallopNetworkState.Online)
            this.chan.PCLeave(this.id);

         try
         {
            this.chan.Close();
            this.chan = null;
            this.factory.Close();
            this.factory = null;
            this.registered = false;
            doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Offline, null, "Logout"));
         }
         catch
         { }
      }

      /// <summary>
      /// Gets the config schema for a PeerChannel network.
      /// </summary>
      public XmlSchema ConfigSchema
      {
         get
         {
            try
            {
               Stream stream = Assembly.GetExecutingAssembly().
                 GetManifestResourceStream("PeerChannel.PeerChannelConfig.xsd");
               XmlSerializer serializer = new XmlSerializer(typeof(XmlSchema));
               return (XmlSchema)(serializer.Deserialize(stream));
            }
            catch
            {
               return null;
            }
         }
      }

      /// <summary>
      /// Gets the node identification.
      /// </summary>
      public string NodeId
      {
         get
         {
            return this.id;
         }
      }

      /// <summary>
      /// Gets the network version.
      /// </summary>
      public string Version
      {
         get
         {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
         }
      }

      /// <summary>
      /// An array containing the neighbouring nodes.
      /// </summary>
      public string[] Neighbors
      {
         get
         {
            return this.oldNeighbours.ToArray();
         }
      }

      /// <summary>
      /// Number of received messages.
      /// </summary>
      public int MessageCountRX
      {
         get
         {
            return this.msgCountRX;
         }
      }

      /// <summary>
      /// Number of sent messages.
      /// </summary>
      public int MessageCountTX
      {
         get
         {
            return this.msgCountTX;
         }
      }

      /// <summary>
      /// Cumulative size of payloads of received messages.
      /// </summary>
      public long MessageSizeRX
      {
         get
         {
            return this.msgSizeRX;
         }
      }

      /// <summary>
      /// Cumulative size of payloads of sent messages.
      /// </summary>
      public long MessageSizeTX
      {
         get
         {
            return this.msgSizeTX;
         }
      }

      /// <summary>
      /// Sum of hopcounts of received messages. Divide by MessageCountRX to get average.
      /// </summary>
      public float HopCountSum
      {
         get
         {
            return this.hopsSumRX;
         }
      }

      /// <summary>
      /// Raised when the network status changes.
      /// </summary>
      public event EventHandler<ScallopNetworkStatusChangedEventArgs> StatusChanged;

      /// <summary>
      /// Raised when a new message is received. The message is passed in the
      /// event arguments.
      /// </summary>
      public event EventHandler<ScallopNetworkDataEventArgs> Data;

      /// <summary>
      /// Raised when the network module wants to inform the user of something.
      /// </summary>
      public event EventHandler<ScallopInfoEventArgs> Info;


      /// <summary>
      /// 
      /// </summary>
      private void registerPeerChannel(string id)
      {
         XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
         quotas.MaxArrayLength = int.MaxValue;
         quotas.MaxBytesPerRead = int.MaxValue;
         quotas.MaxDepth = int.MaxValue;
         quotas.MaxNameTableCharCount = int.MaxValue;
         quotas.MaxStringContentLength = int.MaxValue;

         NetPeerTcpBinding myBinding = new NetPeerTcpBinding();
         myBinding.ReaderQuotas = quotas;

         if (parameters.UseTLS)
         {
            myBinding.Security.Mode = SecurityMode.Transport;
            myBinding.Security.Transport.CredentialType = PeerTransportCredentialType.Password;
         }
         else
         {
            myBinding.Security.Mode = SecurityMode.None;
         }

         // if the listening address is specified, use it
         if (parameters.Ip != null)
         {
            this.doInfo(this, new ScallopInfoEventArgs("Ignored listening address"));
            /*
            this.doInfo(this, new ScallopInfoEventArgs("Listening on ip " + parameters.Ip.ToString()));
            myBinding.ListenIPAddress = parameters.Ip;
            */
         }

         EndpointAddress myAddress = new EndpointAddress("net.p2p://Scallop_" + this.Version + "_" + parameters.NetworkName + "/");

         this.factory = new DuplexChannelFactory<IPeerChannelChannel>(new InstanceContext(this), myBinding, myAddress);
         this.factory.Faulted += this.factory_faulted;
         this.factory.Closed += this.factory_faulted;

         if (parameters.UseTLS)
            this.factory.Credentials.Peer.MeshPassword = parameters.TLSPassword;

         this.chan = factory.CreateChannel();

         ScallopMessageFilter msgFilter = new ScallopMessageFilter(id);
         peer = chan.GetProperty<PeerNode>();
         peer.Offline += new EventHandler(peer_Offline);
         peer.Online += new EventHandler(peer_Online);
         peer.MessagePropagationFilter = msgFilter;

         this.chan.Open();

         this.chan.PCJoin(this.id);
         this.registered = true;

         this.id = id;

         //if(peer.IsOnline)
         //   this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Online, null, "Online"));

      }

      void peer_Online(object sender, EventArgs e)
      {
         this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Online, null, "Online"));
      }

      void peer_Offline(object sender, EventArgs e)
      {
         this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Offline, null, "Offline"));
      }

      #region Event wrappers

      private void doStateChanged(object sender, ScallopNetworkStatusChangedEventArgs e)
      {
         try
         {
            e.OldState = this.myState;
            this.myState = e.NewState;
            if (StatusChanged != null)
               StatusChanged(sender, e);
         }
         catch
         { }
      }

      private void doData(object sender, ScallopNetworkDataEventArgs e)
      {
         try
         {
            this.msgCountRX++;
            this.msgSizeRX += (long)((ScallopMessage)(e.data)).Contents.Length;
            if (Data != null)
               Data(sender, e);
         }
         catch (Exception ee)
         {
            this.doInfo(this, new ScallopInfoEventArgs(ee.ToString()));
         }


      }
      private void doInfo(object sender, ScallopInfoEventArgs e)
      {
         try
         {
            if (Info != null)
               Info(sender, e);
         }
         catch
         { }
      }

      #endregion

      private void factory_faulted(object sender, EventArgs e)
      {
         doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Error, new ApplicationException("Channel factory faulted"), "Channel factory faulted"));
      }

      private void error_handler(object sender, UnhandledExceptionEventArgs e)
      {
         doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(ScallopNetworkState.Error,
           (Exception)e.ExceptionObject, "Error with network."));

         //throw (Exception)e.ExceptionObject;

         /*
         this.Leave();
         Thread.Sleep(5000);
         this.Join(this.configDocument, this.selectedConfig);
         */
      }

      /// <summary>
      /// Gets the network state.
      /// </summary>
      public ScallopNetworkState State
      {
         get
         {
            return this.myState;
         }
      }

      /// <summary>
      /// Sends a message over PeerChannel.
      /// </summary>
      /// <param name="message"></param>
      void IPeerChannel.PCSend(ScallopMessage message)
      {
         // Don't pass own messages or when no handler is registered
         if (message.Header.Sender == this.id || !this.registered || this.Data == null)
            return;


         switch (message.Header.InternalMessage)
         {
            case false:
               if (message.Header.Receivers != null)
               {
                  // the message has a list of receivers, check whether we're on it
                  foreach (string target in message.Header.Receivers)
                  {
                     if (target == this.id)
                     {
                        // alright, we're on it, pass message to user.

                        if (message.Header.OrigHopcount > 0)
                           this.hopsSumRX += (message.Header.OrigHopcount - message.HopCount);
                        doData(this, new ScallopNetworkDataEventArgs(message, "New message"));
                        return;
                     }
                  }
               }
               else // no receiver list, a broadcast message, accept it
               {
                  if (message.Header.OrigHopcount > 0)
                     this.hopsSumRX += (message.Header.OrigHopcount - message.HopCount);
                  doData(this, new ScallopNetworkDataEventArgs(message, "New message"));
                  return;
               }
               break;

            case true:
               if (message.Contents.StartsWith("QUERY"))
               {
                  ScallopMessage msg = new ScallopMessage();
                  msg.Contents = message.Contents;
                  msg.Contents = msg.Contents.Replace("QUERY", "RESPO");
                  msg.Header.Sender = this.id;
                  msg.HopCount = 1;
                  msg.Header.OrigHopcount = 1;
                  msg.Header.InternalMessage = true;
                  this._sendMessage(msg);
               }
               if (message.Contents.StartsWith("RESPO") && message.Contents.Contains(lastNeighbourId))
               {
                  if (!this.neighbours.Contains(message.Header.Sender))
                     this.neighbours.Add(message.Header.Sender);
               }
               break;
         }
      }

      void IPeerChannel.PCJoin(string id)
      {
         this.doInfo(this, new ScallopInfoEventArgs(id + " joined."));
      }

      void IPeerChannel.PCLeave(string id)
      {
         this.doInfo(this, new ScallopInfoEventArgs(id + " left."));
      }
   }
}

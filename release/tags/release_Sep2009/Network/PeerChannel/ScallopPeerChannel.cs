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
using Scallop.Core;
using System.Collections.ObjectModel;

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
         catch (Exception ex)
         {
            throw new ScallopException("Assembly error", ex);
         }
      }

      /// <summary>
      /// Object destructor
      /// </summary>
      ~ScallopPeerChannel()
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
         if (disposing)
         {
            if (this.chan != null)
               this.Leave();
         }
         this.chan = null;
      }
      #endregion

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
         ScallopMessage msg = new ScallopMessage()
         {
            Contents = message,
            HopCount = reach
         };

         this._sendMessage(msg);
      }

      private void _sendMessage(ScallopMessage msg)
      {
         try
         {
            if (this.peer.IsOnline && this.chan != null)
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
         catch (CommunicationException e)
         {
            this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Error, "Error sending message", e));
            //error_handler(this, new UnhandledExceptionEventArgs(e, false));
         }
      }

      /// <summary>
      /// Joins a PeerChannel.
      /// </summary>
      /// <param name="networkSettings">Configuration XML document.</param>
      /// <param name="selectConfig">String identifying config item to use.</param>
      public void Join(XmlDocument networkSettings, string selectConfig)
      {
         if (NetPeerTcpBinding.IsPnrpAvailable == false)
         {
            this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Error, "PNRP is not installed and configured!"));
            return;
         }

         //AppDomain.CurrentDomain.UnhandledException += this.error_handler;

         try
         {
            // parse config
            if (networkSettings == null)
            {
               this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Error, "Config is null"));
               return;
            }

            networkSettings.Schemas.Add(this.configSchema);
            networkSettings.Validate(null);

            this.configDocument = networkSettings;
            this.selectedConfig = selectConfig;

            this.parameters = PeerChannelParameters.ParseConfig(networkSettings, selectConfig);
            if (this.parameters == null)
            {
               throw new ScallopException("StatusChanged parsing config");
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
         BackgroundWorker bw = sender as BackgroundWorker;

         do
         {
            if (bw.CancellationPending == true)
            {
               e.Cancel = true;
               return;
            }

            this.oldNeighbours = new List<string>(this.neighbours); // copy neighbours to oldneighbours
            this.neighbours.Clear();
            this.lastNeighbourId = Guid.NewGuid().ToString();

            ScallopMessage msg = new ScallopMessage()
            {
               Contents = "QUERY" + lastNeighbourId,
               HopCount = 1
            };

            msg.Header.Sender = this.id;
            msg.Header.InternalMessage = true;
            msg.Header.OrigHopcount = 1;
            
            this._sendMessage(msg);
            
            Thread.Sleep(1000 * parameters.NeighborQueryRate);
         } while (true);
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
            doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Offline));
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
            return this.configSchema;
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
      public ReadOnlyCollection<string> Neighbors
      {
         get
         {
            return new ReadOnlyCollection<string>(neighbours);
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
         myBinding.Resolver.Mode = System.ServiceModel.PeerResolvers.PeerResolverMode.Pnrp;

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
         this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Online));
      }

      void peer_Offline(object sender, EventArgs e)
      {
         this.doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Offline));
      }

      #region Event wrappers

      private void doStateChanged(object sender, ScallopNetworkStatusChangedEventArgs e)
      {
         this.myState = e.NewState;
         if (StatusChanged != null)
            StatusChanged(sender, e);
      }

      private void doData(object sender, ScallopNetworkDataEventArgs e)
      {
         this.msgCountRX++;
         this.msgSizeRX += (long)((ScallopMessage)(e.Data)).Contents.Length;

         if (Data != null)
            Data(sender, e);
      }

      private void doInfo(object sender, ScallopInfoEventArgs e)
      {
         if (Info != null)
            Info(sender, e);
      }

      #endregion

      private void factory_faulted(object sender, EventArgs e)
      {
         doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Error, "Channel factory faulted", new ScallopException("Channel factory faulted")));
      }

      private void error_handler(object sender, UnhandledExceptionEventArgs e)
      {
         doStateChanged(this, new ScallopNetworkStatusChangedEventArgs(this.myState, ScallopNetworkState.Error, "Error with network.", (Exception)e.ExceptionObject));

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
         if (message.Header.Sender == this.id || !this.registered)
            return;

         if (message.Header.Receivers != null)
         {
            // the message has a list of receivers, check whether we're on it
            bool bReceive = false;
            
            foreach (string target in message.Header.Receivers)
            {
               if (target == this.id)
               {
                  bReceive = true;
                  break;
               }
            }

            if (!bReceive)
               return;
         }

         if (message.Header.InternalMessage)
         {
            if (message.Contents.StartsWith("QUERY"))
            {
               ScallopMessage msg = new ScallopMessage()
               {
                  Contents = message.Contents.Replace("QUERY", "RESPO"),
                  HopCount = 1,
               };
               
               msg.Header.OrigHopcount = 1;
               msg.Header.InternalMessage = true;
               msg.Header.Sender = this.id;
               msg.Header.Receivers = new string[] { message.Header.Sender };

               this._sendMessage(msg);
            }
            else if (message.Contents.StartsWith("RESPO") && message.Contents.Contains(lastNeighbourId))
            {
               if (!this.neighbours.Contains(message.Header.Sender))
                  this.neighbours.Add(message.Header.Sender);
            }
         }
         else
         {
            if (message.Header.OrigHopcount > 0)
               this.hopsSumRX += (message.Header.OrigHopcount - message.HopCount);
            
            doData(this, new ScallopNetworkDataEventArgs(message, "New message"));
            return;
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

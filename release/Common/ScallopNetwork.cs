﻿using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Specialized;
using System.Xml.Schema;
using System.Xml;
using ScallopCore.Events;

namespace ScallopCore.Network
{
  /// <summary>
  /// Enumeration representing the possible states of a Scallop network
  /// implementation.
  /// </summary>
  public enum ScallopNetworkState { 
    /// <summary>
    /// State indicating the network is idle. No messages are sent or received.
    /// </summary>
    Offline, 
    /// <summary>
    /// State indicating the node has successfully joined the network. Messages
    /// can be sent and received.
    /// </summary>
    Online, 
    /// <summary>
    /// State indicating the network is in an error state. Can indicate network
    /// problems, etc.
    /// </summary>
    Error, 
    /// <summary>
    /// An undefined network state.
    /// </summary>
    Undefined };

  /// <summary>
  /// Network interface for Scallop network implementations.
  /// </summary>
  /// <example>
  /// <code>
  ///  {
  /// ...
  ///  // if the network type is passed from a configuration file 
  ///  IScallopNetwork chan = InterfaceFactory.CreateNetworkInstance("PeerChannel", "Scallop.Network.PeerChannel.ScallopPeerChannel");
  ///  
  ///  if the network type is known at compile time
  ///  // ScallopPeerChannel chan = new ScallopPeerChannel();
  ///  
  ///  chan.Data += this.network_data; // add the handlers
  ///  chan.StatusChanged += this.network_error;
  ///  chan.Info += this.network_info;
  ///  chan.Opened += this.network_join;
  ///  chan.Closed += this.network_leave;
  ///  
  ///  chan.Register(System.Xml.Linq.XDocument.Load("SensorConfig.xml"),
  ///                null);
  ///  
  ///  chan.Join()
  ///  ...
  ///  }
  ///  
  ///  void network_join(object sender, EventArgs e) { }
  ///  void network_leave(object sender, EventArgs e) { }
  ///  void network_error(object sender, ScallopNetworkStatusChangedEventArgs e) { }
  ///  void network_info(object sender, ScallopInfoEventArgs e) {}
  ///  void network_data(object sender, ScallopNetworkDataEventArgs e) {}
  ///  </code>
  /// </example>
  public interface IScallopNetwork : IDisposable
  {
    /// <summary>
    /// Joins a network using the provided settings.
    /// </summary>
    /// <param name="networkSettings">The network settings XML.</param>
    /// <param name="selectConfig">The name of the config to use.</param>
    /// <returns>Node identification string.</returns>
    void Join(XmlDocument networkSettings, string selectConfig);
    
    /// <summary>
    /// Leaves a network.
    /// </summary>
    void Leave();

    /// <summary>
    /// Sends a message, broadcast.
    /// </summary>
    /// <param name="message">The message.</param>
    void SendMessage(string message);

    /// <summary>
    /// Sends a message, to a specific node.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="nodeid">Node ID of the target node.</param>
    void SendMessage(string message, string nodeid);

    /// <summary>
    /// Sends a message, to a group of nodes.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="nodeids"></param>
    void SendMessage(string message, string[] nodeids);

    /// <summary>
    /// Sends a message, to neighbouring nodes within a specified number of hops.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="reach">Hopcount.</param>
    void SendMessage(string message, int reach);

    /// <summary>
    /// Gets the XSD schema used to configure this network type.
    /// </summary>
    /// <returns>An XML schema.</returns>
    XmlSchema ConfigSchema { get; }

    /// <summary>
    /// Gets the network state.
    /// </summary>
    ScallopNetworkState State{ get; }

    /// <summary>
    /// Gets the node identification string.
    /// </summary>
    string NodeId { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets list of node neighbours.
    /// </summary>
    string[] Neighbors { get; }

    /// <summary>
    /// Number of messages received.
    /// </summary>
    int MessageCountRX { get; }

    /// <summary>
    /// Number of messages sent.
    /// </summary>
    int MessageCountTX { get; }

    /// <summary>
    /// Cumulative size of payload in messages received.
    /// </summary>
    ulong MessageSizeRX { get; }

    /// <summary>
    /// Cumulative size of payload in messages sent.
    /// </summary>
    ulong MessageSizeTX { get; }


    /// <summary>
    /// Sum of hopcounts of received messages. Divide by MessageCountRX to get average.
    /// </summary>
    float HopCountSum { get; }
    
    /// <summary>
    /// This event is raised when there is an error with the network.
    /// The reason for the error is contained in the event arguments. 
    /// </summary>
    event ScallopNetworkStatusChangedHandler StatusChanged;

    /// <summary>
    /// This event is raised when new network data is received. The received message
    /// is passed in the event arguments. 
    /// </summary>
    event ScallopNetworkDataHandler Data;

    /// <summary>
    /// This event is raised when the network module wants to inform the user
    /// of something interesting. The message is passed in the event arguments.
    /// </summary>
    event ScallopNetworkInfoHandler Info;
  
  }

  /// <summary> </summary>
  [MessageContract, Serializable]
  public class ScallopMessage
  {
    /// <summary> ID of sending node.</summary>
    [MessageHeader]
    public string sender;

    /// <summary> Content of message in XML.</summary>
    [MessageBodyMember]
    public string contents;

    /// <summary> Array of receiver IDs.</summary>
    [MessageHeader]
    public string[] receivers;

    /// <summary>Hopcount.</summary>
    [PeerHopCount]
    public int hopcount;

    /// <summary>
    /// Original hopcount.
    /// </summary>
    [MessageHeader]
    public int origHopcount;

    /// <summary>
    /// Flag indicating whether this is a message internal to the implementation.
    /// </summary>
    [MessageHeader]
    public bool InternalMessage;

    /*
    /// <summary>XML schema.</summary>
    [MessageProperty]
    public XmlSchema schema;
    */
  }

  

}
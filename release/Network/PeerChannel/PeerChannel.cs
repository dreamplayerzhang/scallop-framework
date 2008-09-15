
using ScallopCore.Network;
using System.Net.PeerToPeer;
using System;
using System.ServiceModel;
namespace Scallop.Network.PeerChannel
{

  /// <summary>
  /// Enumeration of message types.
  /// </summary>
  public enum ScallopPeerChannelMessageType { 
    /// <summary>
    /// Regular message.
    /// </summary>
    Regular, 
    /// <summary>
    /// A neighbour query message.
    /// </summary>
    NeighbourQuery };

  /// <summary>
  /// Interface for PeerChannel communication.
  /// </summary>
  [ServiceContract(CallbackContract = typeof(IPeerChannel))]
  public interface IPeerChannel
  {
    /// <summary>
    /// Method to send a message.
    /// </summary>
    /// <param name="message">The message.</param>
    [OperationContract(IsOneWay = true, IsInitiating = true)]
    void PCSend(ScallopMessage message);

    /// <summary>
    /// Method to join a network.
    /// </summary>
    [OperationContract(IsOneWay = true, IsInitiating = true)]
    void PCJoin(string id);

    /// <summary>
    /// Method to leave a network.
    /// </summary>
    [OperationContract(IsOneWay = true, IsInitiating = true)]
    void PCLeave(string id);
    
  }

  /// <summary>
  /// Inner communication channel.
  /// </summary>
  public interface IPeerChannelChannel : IPeerChannel, IClientChannel
  {
  }
  
  

}
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

using Scallop.Core.Network;
using System.ServiceModel;

namespace Scallop.Network.PeerChannel
{

   /// <summary>
   /// Enumeration of message types.
   /// </summary>
   public enum ScallopPeerChannelMessageType
   {
      /// <summary>
      /// Regular message.
      /// </summary>
      Regular,
      /// <summary>
      /// A neighbour query message.
      /// </summary>
      NeighbourQuery
   };

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
      [OperationContract(IsOneWay = true)]
      void PCSend(ScallopMessage message);

      /// <summary>
      /// Method to join a network.
      /// </summary>
      [OperationContract(IsOneWay = true)]
      void PCJoin(string id);

      /// <summary>
      /// Method to leave a network.
      /// </summary>
      [OperationContract(IsOneWay = true)]
      void PCLeave(string id);

   }

   /// <summary>
   /// Inner communication channel.
   /// </summary>
   public interface IPeerChannelChannel : IPeerChannel, IClientChannel
   {
   }



}
﻿/*
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

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Scallop.Network.PeerChannel
{
   /// <summary>
   /// A message filter that filters out own sent messages
   /// </summary>
   internal class ScallopMessageFilter : PeerMessagePropagationFilter
   {
      private string ownID = "";

      /// <summary>
      /// Own node ID
      /// </summary>
      public string ID 
      {
         get { return ownID; }
         set { ownID = value; }
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="strID"></param>
      public ScallopMessageFilter(string strID)
      {
         this.ownID = strID;
      }

      /// <summary>
      /// Delivers only the messages sent by other nodes
      /// </summary>
      /// <param name="message">Incoming message</param>
      /// <param name="origination">Message source</param>
      /// <returns></returns>
      public override PeerMessagePropagation ShouldMessagePropagate(Message message, PeerMessageOrigination origination)
      {

         PeerMessagePropagation destination = PeerMessagePropagation.LocalAndRemote;

         if (origination == PeerMessageOrigination.Local)
            destination = PeerMessagePropagation.Remote;

         return destination;

      }
   }
}

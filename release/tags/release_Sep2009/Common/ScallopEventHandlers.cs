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
using Scallop.Core.Network;
using Scallop.Core.Sensor;

namespace Scallop.Core.Events
{
   #region Sensor

   ///// <summary>
   ///// Event handler signature for sensor open events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, empty.</param>
   //public delegate void ScallopSensorOpenedHandler(object sender, EventArgs e);

   ///// <summary>
   ///// Event handler signature for sensor close events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, empty.</param>
   //public delegate void ScallopSensorClosedHandler(object sender, EventArgs e);

   ///// <summary>
   ///// Event handler signature for sensor error events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, containing the error message.</param>
   //public delegate void ScallopSensorStatusChangedHandler(object sender, ScallopSensorStatusChangedEventArgs e);

   ///// <summary>
   ///// Event handler signature for sensor data events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, containing the data associated with the event.</param>
   //public delegate void ScallopSensorDataHandler(object sender, ScallopSensorDataEventArgs e);

   /// <summary>
   /// Event arguments for sensor status changed events.
   /// </summary>
   public class ScallopSensorStatusChangedEventArgs : EventArgs
   {
      /// <summary>
      /// Possible exception that caused the error.
      /// </summary>
      private Exception causingException;

      /// <summary>
      /// Gets the possible exception that caused the error.
      /// </summary>
      public Exception CausingException
      {
         get { return causingException; }
      }

      /// <summary>
      /// StatusChanged message.
      /// </summary>
      private string msg;

      /// <summary>
      /// Gets the StatusChanged message.
      /// </summary>
      public string Message
      {
         get { return msg; }
      }

      /// <summary>
      /// The state after the event.
      /// </summary>
      private ScallopSensorState newState;

      /// <summary>
      /// Gets the state after the event.
      /// </summary>
      public ScallopSensorState NewState
      {
         get { return newState; }
      }

      /// <summary>
      /// The state before the event.
      /// </summary>
      private ScallopSensorState oldState;

      /// <summary>
      /// Gets the state before the event.
      /// </summary>
      public ScallopSensorState OldState
      {
         get { return oldState; }
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="fromState">State before event.</param>
      /// <param name="toState">State after event.</param>
      public ScallopSensorStatusChangedEventArgs(ScallopSensorState fromState, ScallopSensorState toState)
         : this(fromState, toState, null, null)
      {

      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="fromState">State before event.</param>
      /// <param name="toState">State after event.</param>
      /// <param name="msg">A freeform string message for the user.</param>
      public ScallopSensorStatusChangedEventArgs(ScallopSensorState fromState, ScallopSensorState toState, string msg)
         : this(fromState, toState, msg, null)
      {

      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="fromState">State before event.</param>
      /// <param name="toState">State after event.</param>
      /// <param name="msg">A freeform string message for the user.</param>
      /// <param name="causingException">Possible exception that caused an error.</param>
      public ScallopSensorStatusChangedEventArgs(ScallopSensorState fromState, ScallopSensorState toState, string msg, Exception causingException)
      {
         this.oldState = fromState;
         this.newState = toState;
         this.causingException = causingException;
         this.msg = msg;
      }
   }

   /// <summary>
   /// Event arguments for sensor data events.
   /// </summary>
   public class ScallopSensorDataEventArgs : EventArgs
   {
      private object data;

      /// <summary>
      /// The data content.
      /// </summary>
      public object Data
      {
         get { return data; }
      }

      /// <summary>
      /// Type of data.
      /// </summary>
      public Type DataType
      {
         get { return this.data.GetType(); }
      }

      private string msg;

      /// <summary>
      /// A freeform message for the user.
      /// </summary>
      public string Message
      {
         get { return msg; }
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="data">The event sensordata.</param>
      /// <param name="msg">A message.</param>
      public ScallopSensorDataEventArgs(object data, string msg)
      {
         this.data = data;
         this.msg = msg;
      }
   }

   ///// <summary>
   ///// Event handler signature for sensor info events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, containing the information text.</param>
   //public delegate void ScallopSensorInfoHandler(object sender, ScallopInfoEventArgs e);

   /// <summary>
   /// Event arguments for sensor and network information events.
   /// </summary>
   public class ScallopInfoEventArgs : EventArgs
   {
      private string msg;

      /// <summary>
      /// The information message.
      /// </summary>
      public string Message
      {
         get { return msg; }
         set { msg = value; }
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="msg">The information message.</param>
      public ScallopInfoEventArgs(string msg)
      {
         this.msg = msg;
      }
   }

   #endregion Sensor

   #region Network

   ///// <summary>
   ///// Event handler signature for network open events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, empty.</param>
   //public delegate void ScallopNetworkOpenedHandler(object sender, EventArgs e);

   ///// <summary>
   ///// Event handler signature for network closed events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, empty.</param>
   //public delegate void ScallopNetworkClosedHandler(object sender, EventArgs e);


   ///// <summary>
   ///// Event handler signature for network error events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, containing the error message.</param>
   //public delegate void ScallopNetworkStatusChangedHandler(object sender, ScallopNetworkStatusChangedEventArgs e);

   /// <summary>
   /// Event arguments for network status changed events.
   /// </summary>
   public class ScallopNetworkStatusChangedEventArgs : EventArgs
   {

      private Exception causingException;

      /// <summary>
      /// Possible exception that caused the error.
      /// </summary>
      public Exception CausingException
      {
         get { return causingException; }
      }

      private string msg;

      /// <summary>
      /// StatusChanged message.
      /// </summary>
      public string Message
      {
         get { return msg; }
      }

      private ScallopNetworkState newState;

      /// <summary>
      /// State after the event.
      /// </summary>
      public ScallopNetworkState NewState
      {
         get { return newState; }
      }

      private ScallopNetworkState oldState;

      /// <summary>
      /// State before the event.
      /// </summary>
      public ScallopNetworkState OldState
      {
         get { return oldState; }
         set { oldState = value; }
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="fromState">State before event.</param>
      /// <param name="toState">State after event.</param>
      public ScallopNetworkStatusChangedEventArgs(ScallopNetworkState fromState, ScallopNetworkState toState)
         : this(fromState, toState, null, null)
      {

      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="fromState">State before event.</param>
      /// <param name="toState">State after event.</param>
      /// <param name="msg">A freeform string message for the user.</param>
      public ScallopNetworkStatusChangedEventArgs(ScallopNetworkState fromState, ScallopNetworkState toState, string msg)
         : this(fromState, toState, msg, null)
      {

      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="fromState">State before event.</param>
      /// <param name="toState">State after event.</param>
      /// <param name="msg">A freeform string message for the user.</param>
      /// <param name="causingException">Possible exception that caused an error.</param>
      public ScallopNetworkStatusChangedEventArgs(ScallopNetworkState fromState, ScallopNetworkState toState, string msg, Exception causingException)
      {
         this.oldState = fromState;
         this.newState = toState;
         this.causingException = causingException;
         this.msg = msg;
      }
   }

   ///// <summary>
   ///// Event handler signature for network data events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event arguments, containing the data.</param>
   //public delegate void ScallopNetworkDataHandler(object sender, ScallopNetworkDataEventArgs e);

   /// <summary>
   /// Event arguments for network data events.
   /// </summary>
   public class ScallopNetworkDataEventArgs : EventArgs
   {
      private object data;

      /// <summary>
      /// The data received from the network.
      /// </summary>
      public object Data
      {
         get { return data; }
      }

      /// <summary>
      /// Type of data.
      /// </summary>
      public Type DataType
      {
         get { return this.data.GetType(); }
      }

      private string msg;

      /// <summary>
      /// An optional message for the user.
      /// </summary>
      public string Message
      {
         get { return msg; }
         set { msg = value; }
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="data">The network data.</param>
      /// <param name="msg">A user message.</param>
      public ScallopNetworkDataEventArgs(object data, string msg)
      {
         this.data = data;
         this.msg = msg;
      }
   }

   ///// <summary>
   ///// Event handler signature for network info events.
   ///// </summary>
   ///// <param name="sender">Identifies the object that sent the event.</param>
   ///// <param name="e">Event parameters, containing the information text.</param>
   //public delegate void ScallopNetworkInfoHandler(object sender, ScallopInfoEventArgs e);

   #endregion Network
}

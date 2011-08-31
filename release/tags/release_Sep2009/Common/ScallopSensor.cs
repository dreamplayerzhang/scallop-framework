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
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;

using Scallop.Core.Events;

namespace Scallop.Core.Sensor
{

   /// <summary>
   /// An enumeration of possible sensor states
   /// </summary>
   public enum ScallopSensorState
   {
      /// <summary>
      /// The sensor is in an idle state. No data is gathered or sent.
      /// </summary>
      Idle,
      /// <summary>
      /// The sensor is active and gathering and passing new data as it becomes
      /// available.
      /// </summary>
      Active,
      /// <summary>
      /// The sensor is in an error state. This might indicate problems with the
      /// sensor.
      /// </summary>
      Error,
      /// <summary>
      /// Catch-all undefined state.
      /// </summary>
      Undefined
   };


   /// <summary>
   /// Generic sensor class for the Scallop system.
   /// </summary>
   public interface IScallopSensor : IDisposable
   {
      ///// <summary>
      ///// Register the node's interest in a sensor.
      ///// </summary>
      ///// <param name="config">The sensor settings, including
      ///// address, resolution, frequency of updates etc.</param>
      ///// <param name="selectConfig">String containing the config name that should
      ///// be used.</param>
      //void Register(XmlDocument config, string selectConfig);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="config">The sensor settings, including
      /// address, resolution, frequency of updates etc.</param>
      /// /// <param name="selectConfig">String containing the config name that should
      /// be used.</param>
      void Register(XDocument config, string selectConfig);

      /// <summary>
      /// Starts receiving sensor data.
      /// </summary>
      void Start();

      /// <summary>
      /// Stops receiving sensor data
      /// </summary>
      void Stop();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="absolute"></param>
      /// <param name="xCoordinate"></param>
      /// <param name="yCoordinate"></param>
      void PanTilt(bool absolute, int xCoordinate, int yCoordinate);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="absolute"></param>
      /// <param name="zoomValue"></param>
      void Zoom(bool absolute, int zoomValue);

      /// <summary>
      /// 
      /// </summary>
      string Version { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      XmlSchema ConfigSchema { get; }

      /// <summary>
      /// Gets the sensor state.
      /// </summary>
      ScallopSensorState State { get; }

      /// <summary>
      /// This event is raised when the status of the sensor changes. This can
      /// mean the sensor becoming active, encountering an error condition, etc.
      /// </summary>
      //event ScallopSensorStatusChangedHandler StatusChanged;
      event EventHandler<ScallopSensorStatusChangedEventArgs> StatusChanged;

      /// <summary>
      /// This event is raised when the sensor has new data available. The data is
      /// passed in the event arguments.
      /// </summary>
      //event ScallopSensorDataHandler Data;
      event EventHandler<ScallopSensorDataEventArgs> Data;

      /// <summary>
      /// This event is raised when the sensor has information that the user might
      /// find useful. The information is passed in the event arguments.
      /// </summary>
      //event ScallopSensorInfoHandler Info;
      event EventHandler<ScallopInfoEventArgs> Info;

   }
}
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
using System.Reflection;

using Scallop.Core.Sensor;
using Scallop.Core.Network;

[assembly:System.CLSCompliant(true)]

namespace Scallop.Core
{
  // TODO Handle exceptions.

  /// <summary>
  /// Creates new sensor and network instances from assembly and class names.
  /// </summary>
  public abstract class InterfaceFactory
  {
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly">The assembly containing the sensor class.</param>
    /// <param name="sensorType">Name of the sensor class, including namespace.</param>
    /// <returns>Sensor instance that was created.</returns>
    public static IScallopSensor CreateSensorInstance(string assembly,
                                                      string sensorType )
    {
      return (IScallopSensor)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(assembly,sensorType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly">The assembly containing the sensor class.</param>
    /// <returns>Sensor instance that was created.</returns>
    public static IScallopSensor CreateSensorInstance(string assembly)
    {
       string sensorType = null;
       Assembly asm = Assembly.LoadFrom(assembly);

       Type[] types = asm.GetTypes();

       foreach (Type t in types)
       {
          Type sensorInterface = t.GetInterface("Scallop.Core.Sensor.IScallopSensor");

          if (sensorInterface != null)
          {
             sensorType = t.FullName;
             break;
          }
       }

       return InterfaceFactory.CreateSensorInstance(assembly, sensorType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly">The assembly containing the network class.</param>
    /// <param name="networkType">Name of the network class, including namespace.</param>
    /// <returns>Network instance that was created.</returns>
    public static IScallopNetwork CreateNetworkInstance(string assembly,
                                                        string networkType)
    {
      return (IScallopNetwork)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(assembly, networkType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly">The assembly containing the sensor class.</param>
    /// <returns>Network instance that was created.</returns>
    public static IScallopNetwork CreateNetworkInstance(string assembly)
    {
       string sensorType = null;
       Assembly asm = Assembly.LoadFrom(assembly);

       Type[] types = asm.GetTypes();

       foreach (Type t in types)
       {
          Type sensorInterface = t.GetInterface("Scallop.Core.Network.IScallopNetwork");

          if (sensorInterface != null)
          {
             sensorType = t.FullName;
             break;
          }
       }

       return InterfaceFactory.CreateNetworkInstance(assembly, sensorType);
    }
  }
}

using System;
using ScallopCore;
using ScallopCore.Sensor;
using ScallopCore.Network;

namespace ScallopCore
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
    /// <returns></returns>
    public static IScallopSensor CreateSensorInstance(string assembly,
                                                      string sensorType )
    {
      return (IScallopSensor)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assembly,sensorType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assembly">The assembly containing the network class.</param>
    /// <param name="networkType">Name of the network class, including namespace.</param>
    /// <returns></returns>
    public static IScallopNetwork CreateNetworkInstance(string assembly,
                                                        string networkType)
    {
      return (IScallopNetwork)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assembly, networkType);
    }

  }
}

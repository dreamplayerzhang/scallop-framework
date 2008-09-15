using System;
using System.Collections.Specialized;
using System.Xml.Schema;
using System.Xml;
using ScallopCore.Events;

namespace ScallopCore.Sensor
{

  /// <summary>
  /// An enumeration of possible sensor states
  /// </summary>
  public enum ScallopSensorState { 
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
    Undefined };


  /// <summary>
  /// Generic sensor class for the Scallop system.
  /// </summary>
  public interface IScallopSensor : IDisposable
  {
    /// <summary>
    /// Register the node's interest in a sensor.
    /// </summary>
    /// <param name="config">The sensor settings, including
    /// address, resolution, frequency of updates etc.</param>
    /// <param name="selectConfig">String containing the config name that should
    /// be used.</param>
    void Register(XmlDocument config, string selectConfig);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="config">The sensor settings, including
    /// address, resolution, frequency of updates etc.</param>
    /// /// <param name="selectConfig">String containing the config name that should
    /// be used.</param>
    void Register(System.Xml.Linq.XDocument config, string selectConfig);

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
    /// <param name="x"></param>
    /// <param name="y"></param>
    void PanTilt(bool absolute, int x, int y);

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
    event ScallopSensorStatusChangedHandler StatusChanged;

    /// <summary>
    /// This event is raised when the sensor has new data available. The data is
    /// passed in the event arguments.
    /// </summary>
    event ScallopSensorDataHandler Data;

    /// <summary>
    /// This event is raised when the sensor has information that the user might
    /// find useful. The information is passed in the event arguments.
    /// </summary>
    event ScallopSensorInfoHandler Info;

  }

 


}

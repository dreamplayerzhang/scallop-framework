using System.Runtime.Serialization;
using System;

//
// For guidelines regarding the creation of new exception types, see
//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
// and
//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
//

/// <summary>
/// A generic Scallop exception.
/// </summary>
public class ScallopException : ApplicationException
{
  /// <summary>
  /// Default constructor.
  /// </summary>
  public ScallopException() { }

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="message">Message to user.</param>
  public ScallopException(string message) : base(message) { }

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="message">Message to user.</param>
  /// <param name="inner">A possible causing InnerException.</param>
  public ScallopException(string message, Exception inner) : base(message, inner) { }
  
}

/// <summary>
/// Thrown when the XML configuration is invalid.
/// </summary>
public class InvalidConfigurationException : ScallopException
{
  /// <summary>
  /// Default constructor.
  /// </summary>
  public InvalidConfigurationException() { }

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="message">Message to user.</param>
  public InvalidConfigurationException(string message) : base(message) { }

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="message">Message to user.</param>
  /// <param name="inner">A possible causing InnerException.</param>
  public InvalidConfigurationException(string message, Exception inner) : base(message, inner) { }
  
}

/// <summary>
/// Thrown when a message content does not match the XML schema.
/// </summary>
public class MessageContentException : ScallopException
{
  static string defaultMessage = "The message content does not match the XML schema.";

  /// <summary>
  /// Default constructor.
  /// </summary>
  public MessageContentException()
  {
    new MessageContentException(defaultMessage);
  }

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="inner">Message to user.</param>
  public MessageContentException(System.Exception inner) : base(defaultMessage, inner) { }


  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="message">Message to user.</param>
  public MessageContentException(string message) : base(message) { }

  /// <summary>
  /// Constructor.
  /// </summary>
  /// <param name="message">Message to user.</param>
  /// <param name="inner">A possible causing InnerException.</param>
  public MessageContentException(string message, System.Exception inner) : base(message, inner) { }

  
  
}
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
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
namespace ScallopCore
{
  /// <summary>
  /// Static utilities for Scallop. Includes Base64Encoding.
  /// </summary>
  public class ScallopUtils
  {
    
    /// <summary>
    /// Base64-encodes a byte buffer to a format that is suitable for text-based transmission
    /// </summary>
    /// <param name="buf">Data to be encoded.</param>
    /// <returns>A string containing the data in buf, encoded in base64.</returns>
    /// <remarks>Use Base64Decode to reverse the encoding.</remarks>
    public static string Base64Encode(byte[] buf)
    {
      if (buf != null)
        return (Convert.ToBase64String(buf));
      else
        return null;
    }


    /// <summary>
    /// Base64 decodes a string to it's byte buffer representation.
    /// </summary>
    /// <param name="encoded">The encoded string.</param>
    /// <returns>Byte buffer containing decoded data.</returns>
    public static byte[] Base64Decode(string encoded)
    {
      byte[] buf;
      try
      {
        buf = Convert.FromBase64String(encoded);
        return buf;
      }
      catch
      {
        return null;
      }
    }

  }
}
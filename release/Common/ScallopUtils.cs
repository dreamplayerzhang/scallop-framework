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
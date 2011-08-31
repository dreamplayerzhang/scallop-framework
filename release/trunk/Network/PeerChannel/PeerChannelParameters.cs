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

using System.Xml;
using System;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

using Scallop.Core;

namespace Scallop.Network.PeerChannel
{
  class PeerChannelParameters
  {
    public string NodeId = null;
    public string NetworkName;
    public IPAddress Ip = null;
    public bool UseTLS = false;
    public string TLSPassword = null;
    public int NeighborQueryRate = 5;

    public static PeerChannelParameters ParseConfig(XmlDocument configDoc, string selectConfig)
    {
      PeerChannelParameters parameters = new PeerChannelParameters();
      
      XmlNodeList configs = configDoc.GetElementsByTagName("PeerChannelConfigSet")[0].ChildNodes;
      XmlNode elementList = configs.Item(0);

      string defConfig = configDoc.DocumentElement.Attributes.GetNamedItem("DefaultConfig").InnerText;
      if (selectConfig != null)
        defConfig = selectConfig;

      while (elementList.Attributes.GetNamedItem("ConfigName").InnerText != defConfig)
      {
        elementList = elementList.NextSibling;
        if (elementList == null)
          throw new ScallopException("Configuration " + defConfig + " not found!");
      }

      XmlNodeList settings = elementList.ChildNodes;
      for (int i = 0; i < settings.Count; i++)
      {
        XmlNode setting = settings.Item(i);
        switch (setting.Name)
        {
          case "NodeId":
            parameters.NodeId = setting.InnerText;
            break;

          case "NetworkName":
            parameters.NetworkName = setting.InnerText;
            break;

          case "NetworkAddress":
            parameters.Ip = IPAddress.Parse(setting.InnerText);
            break;

          case "EnableTransportSecurity":
            parameters.UseTLS = bool.Parse(setting.InnerText);
            break;

          case "TLSPassword":
            parameters.TLSPassword = setting.InnerText;
            break;

          case "NeighborQueryRate":
            parameters.NeighborQueryRate = int.Parse(setting.InnerText);
            break;

          default:
            break;

        }
      }

      return parameters;
    }
  }

}
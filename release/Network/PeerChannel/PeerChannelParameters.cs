using System.Xml;
using System;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

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
          throw new ApplicationException("Configuration " + defConfig + " not found!");
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
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
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Scallop.Sensor.Axis
{

  /// <summary>
  /// 
  /// </summary>
  class AxisParameters
  {
    // Parameters from 
    // http://www.axis.com/techsup/cam_servers/dev/cam_http_api_2.php#api_blocks_image_video_mjpg_video_cgi
    
    public string Address;
    public string Resolution = null;
    public int Camera = -1; // no point in the quad option
    public int Compression = -1;
    
    public int UseColor = -1;
    public int ColorLevel = -1;
    
    public int ShowClock = -1; 
    public int ShowDate = -1;
    
    public int ShowText = -1;
    public string Text = null;
    public string TextColor = null;
    public string TextBackGround = null;
    public string TextPosition = null;

    public int ShowOverlay = -1;
    public string OverlayPosition = null;
    
    public int Fps = -1;
    public int SquarePixel = -1;

    public int Rotation = -1;

    public string UserName = null;
    public string Password = null;

    public string delimiter = "--myboundary";
    public string frameFormat = null;


    public string MjpgParameterString()
    {
      string parameters = 
        /*"http://" +*/ this.Address + "/axis-cgi/mjpg/video.cgi?" +
        ((this.Fps != -1 ) ? "fps=" + this.Fps : "" ) +
        ((this.Resolution != null ) ? "&resolution=" + this.Resolution : "") +
        ((this.Compression != -1 ) ? "&compression=" + this.Compression : "")+
        ((this.UseColor != -1 ) ? "&color=" + this.UseColor : "") +
        ((this.ShowClock != -1 ) ? "&clock=" + this.ShowClock : "" )+
        ((this.ShowDate != -1 ) ? "&date=" + this.ShowDate : "")+
        ((this.ShowText != -1 ) ? "&text=" + this.ShowText : "" ) +
        ((this.TextColor != null ) ? "&textcolor=" + this.TextColor : "" )+
        ((this.TextBackGround != null ) ? "&textbackgroundcolor=" + this.TextBackGround : "") +
        ((this.TextPosition != null ) ? "&textposition=" + this.TextPosition : "") +
        ((this.Text != null ) ? "&textstring=" + this.Text : "") +
        ((this.Rotation != -1 ) ? ("&rotation=" + this.Rotation) : "") +
        ((this.Camera != -1 ) ? "&camera=" + this.Camera : "") +
        ((this.ColorLevel != -1 ) ? "&colorlevel=" + this.ColorLevel : "") +
        ((this.ShowOverlay != -1 ) ? "&overlayimage=" + this.ShowOverlay : "") + 
        ((this.OverlayPosition != null ) ? "&overlayposition=" + this.OverlayPosition : "")
        ;

      return parameters;
    }

    public static AxisParameters ParseConfig(XDocument configDoc, string selectConfig)
    {
      AxisParameters parameters = new AxisParameters();


      XElement root = configDoc.Root;
      string defConfig = root.Attribute("DefaultConfig").Value.ToString();
      if (selectConfig == null)
        selectConfig = defConfig;

      IEnumerable<XElement> configs =
        from el in root.Elements(XName.Get("AxisCameraConfig", "Scallop/AxisCameraSchema.xsd"))
        where el.Attribute("ConfigName").Value == selectConfig
        select el;

      if ( configs.Count<XElement>() != 1 )
        throw new ApplicationException("Ambiguous config name, " + selectConfig);

      XElement activeConfig = configs.First<XElement>();

      parameters.Address = getParamValue(activeConfig, "Address", "string");
      parameters.frameFormat = getParamValue(activeConfig,"FrameFormat","string");
      parameters.OverlayPosition = getParamValue(activeConfig, "OverlayPosition", "string");
      parameters.Password = getParamValue(activeConfig, "Password", "string");
      parameters.Resolution = getParamValue(activeConfig, "Resolution", "string");
      parameters.Text = getParamValue(activeConfig, "Text", "string");
      parameters.TextBackGround = getParamValue(activeConfig, "TextBackgroundColor", "string");
      parameters.TextColor = getParamValue(activeConfig, "TextColor", "string");
      parameters.TextPosition = getParamValue(activeConfig, "TextPosition", "string");
      parameters.UserName = getParamValue(activeConfig, "User", "string");

      parameters.Camera = int.Parse(getParamValue(activeConfig, "Camera", "int"));
      parameters.ColorLevel = int.Parse(getParamValue(activeConfig, "ColorLevel", "int"));
      parameters.Compression = int.Parse(getParamValue(activeConfig, "Compression", "int"));
      parameters.Fps = int.Parse(getParamValue(activeConfig, "Framerate", "int"));
      parameters.Rotation = int.Parse(getParamValue(activeConfig, "Rotation", "int"));
      parameters.ShowClock = int.Parse(getParamValue(activeConfig, "ShowClock", "int"));
      parameters.ShowDate = int.Parse(getParamValue(activeConfig, "ShowDate", "int"));
      parameters.ShowOverlay = int.Parse(getParamValue(activeConfig, "ShowOverLay", "int"));
      parameters.ShowText = int.Parse(getParamValue(activeConfig, "ShowText", "int"));
      
      return parameters;
    }

    private static string getParamValue(XElement config, string item, string type)
    {
      int tmp;
      XElement target = config.Element(XName.Get(item, "Scallop/AxisCameraSchema.xsd"));
      string value;
      value = (target == null ? null : target.Value);
      switch (type)
      {
        case "int":
          return (int.TryParse(value, out tmp) ? target.Value : "-1");
        case "string":
          return (value);
      }
      
      return target.Value;
    }
 
    
    public static AxisParameters ParseConfig(XmlDocument configDoc, string selectConfig)
    {
      AxisParameters parameters = new AxisParameters();

      
      XmlNodeList configs = configDoc.GetElementsByTagName("AxisCameraConfigSet")[0].ChildNodes;
      XmlNode elementList = configs.Item(0);
      
      string defConfig = configDoc.DocumentElement.Attributes.GetNamedItem("DefaultConfig").InnerText;
      if (selectConfig != null)
        defConfig = selectConfig;

      while ( elementList.Attributes.GetNamedItem("ConfigName").InnerText != defConfig )
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
          case "Address":
            parameters.Address = setting.InnerText.Trim();
            break;
      
          case "Color":
            parameters.UseColor =
              (setting.InnerText == "true" || setting.InnerText == "1") ? 1 : 0;
            break;

          case "ColorLevel":
            parameters.ColorLevel = int.Parse(setting.InnerText.Trim());
            break;

          case "Compression":
            parameters.Compression = int.Parse(setting.InnerText.Trim());
            break;
            
          case "Framerate":
            parameters.Fps = int.Parse(setting.InnerText.Trim());
            break;

          case "OverlayPosition":
            parameters.OverlayPosition = setting.InnerText.Trim();
            break;

          case "Password":
            parameters.Password = setting.InnerText;
            break;
            
          case "Resolution":
            parameters.Resolution = setting.InnerText.Trim();
            break;

          case "Rotation":
            parameters.Rotation = int.Parse(setting.InnerText.Trim());
            break;
            
          case "ShowClock":
            parameters.ShowClock = 
              (setting.InnerText == "true" || setting.InnerText == "1") ? 1 : 0;
            break;
            
          case "ShowDate":
            parameters.ShowDate =
              (setting.InnerText == "true" || setting.InnerText == "1") ? 1 : 0;
            break;

          case "ShowOverlay":
            parameters.ShowOverlay =
              (setting.InnerText == "true" || setting.InnerText == "1") ? 1 : 0;
            break;
            
          case "ShowText":
            parameters.ShowText =
              (setting.InnerText == "true" || setting.InnerText == "1") ? 1 : 0;
            break;

          case "Text":
            parameters.Text = setting.InnerText;
            break;

          case "TextBackgroundColor":
            parameters.TextBackGround = setting.InnerText.Trim();
            break;
          
          case "TextColor":
            parameters.TextColor = setting.InnerText.Trim();
            break;

          case "TextPosition":
            parameters.TextPosition = setting.InnerText.Trim();
            break;

          case "User":
            parameters.UserName = setting.InnerText;
            break;

          case "FrameFormat":
            parameters.frameFormat = setting.InnerText;
            break;

          default:
            break;

        }
      }

      return parameters;
    }
  }

}
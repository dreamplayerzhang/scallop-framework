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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Scallop.Core;

namespace Scallop.Sensor.FileSource
{
  class FileSourceParameters
  {
    public Uri SourceUri;
    public string FrameFormat;
 
    public static FileSourceParameters ParseConfig(XDocument configDoc, string selectConfig)
    {
      FileSourceParameters parameters = new FileSourceParameters();


      XElement root = configDoc.Root;
      string defConfig = root.Attribute("DefaultConfig").Value.ToString();
      if (selectConfig == null)
        selectConfig = defConfig;

      IEnumerable<XElement> configs =
        from el in root.Elements(XName.Get("FileSourceConfig", "Scallop/FileSourceSchema.xsd"))
        where el.Attribute("ConfigName").Value == selectConfig
        select el;

      if (configs.Count<XElement>() != 1)
        throw new ScallopException("Ambiguous config name, " + selectConfig);

      XElement activeConfig = configs.First<XElement>();

      parameters.SourceUri = new Uri(getParamValue(activeConfig, "SourceUri", "string"),UriKind.RelativeOrAbsolute);
      parameters.FrameFormat = getParamValue(activeConfig, "FrameFormat", "string");  

      return parameters;
    }

    private static string getParamValue(XElement config, string item, string type)
    {
      int tmp;
      XElement target = config.Element(XName.Get(item, "Scallop/FileSourceSchema.xsd"));
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
 
  }
}

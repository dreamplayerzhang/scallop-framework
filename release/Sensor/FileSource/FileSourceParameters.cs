using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
        throw new ApplicationException("Ambiguous config name, " + selectConfig);

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

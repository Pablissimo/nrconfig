using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewRelicConfigManager.Rendering
{
    [XmlRoot(ElementName = "extension", Namespace = "urn:newrelic-extension")]
    public class Extension
    {
        [XmlElement(ElementName="instrumentation")]
        public Instrumentation Instrumentation { get; set; }
    }
}

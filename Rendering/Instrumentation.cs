using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewRelicConfigManager.Rendering
{
    public class Instrumentation
    {
        [XmlElement(ElementName="tracerFactory")]
        public List<TracerFactory> TracerFactories { get; set; }

        public Instrumentation()
        {
            this.TracerFactories = new List<TracerFactory>();
        }
    }
}

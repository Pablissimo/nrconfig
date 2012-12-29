using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewRelicConfigManager.Rendering
{
    public class TracerFactory
    {
        [XmlAttribute(AttributeName="metricName")]
        public string MetricName { get; set; }
        [XmlElement(ElementName="match")]
        public List<Match> MatchDefinitions { get; set; }

        public TracerFactory()
        {
            this.MatchDefinitions = new List<Match>();
        }

        public TracerFactory(string metricName)
            : this()
        {
            this.MetricName = metricName;
        }
    }
}

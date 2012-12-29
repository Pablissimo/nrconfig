using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewRelicConfigManager.Rendering
{
    public class Match
    {
        [XmlAttribute(AttributeName="metric")]
        public string Metric { get; set; }
        [XmlAttribute(AttributeName="assemblyName")]
        public string AssemblyName { get; set; }
        [XmlAttribute(AttributeName = "className")]
        public string ClassName { get; set; }

        [XmlElement(ElementName="match")]
        public List<ExactMethodMatcher> Matches { get; set; }

        public Match()
        {
            this.Matches = new List<ExactMethodMatcher>();
        }

        public Match(string metric, string assemblyName, string className)
            : this()
        {
            this.Metric = metric;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
        }
    }
}

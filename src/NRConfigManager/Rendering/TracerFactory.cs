using NRConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NRConfigManager.Rendering
{
    /// <summary>
    /// /// Class for New Relic-compatible XML output of the tracerFactory element
    /// </summary>
    public class TracerFactory
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "transactionNamingPriority")]
        public string TransactionNamingPriority { get; set; }
        [XmlAttribute(AttributeName="metricName")]
        public string MetricName { get; set; }
        [XmlAttribute(AttributeName="metric")]
        [DefaultValue(Metric.Unspecified)]
        public Metric Metric { get; set; }
        [XmlElement(ElementName="match")]
        public List<Match> MatchDefinitions { get; set; }

        public TracerFactory()
        {
            this.MatchDefinitions = new List<Match>();
        }

        public TracerFactory(string metricName, string name, string transactionNamingPriority, Metric metric)
            : this()
        {
            this.Name = name;
            this.TransactionNamingPriority = transactionNamingPriority;
            this.MetricName = metricName;
            this.Metric = metric;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Rendering
{
    public class TracerFactory
    {
        public string MetricName { get; set; }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfiguration
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Assembly)]
    public class InstrumentAttribute : Attribute
    {
        public string MetricName { get; set; }
        public Metric Metric { get; set; }
        public InstrumentationScopes Scopes { get; set; }

        public InstrumentAttribute()
            : this(null)
        {
            this.Scopes = InstrumentationScopes.All;
        }

        public InstrumentAttribute(string metricName)
            : this(metricName, Metric.Unspecified)
        {
            this.MetricName = metricName;
        }

        public InstrumentAttribute(string metricName, Metric metric)
        {
            this.Scopes = InstrumentationScopes.All;

            this.MetricName = metricName;
            this.Metric = metric;
        }
    }
}

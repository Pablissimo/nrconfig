using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Configuration
{
    public class InstrumentationTarget
    {
        public MethodInfo Method { get; private set; }
        public string MetricName { get; private set; }
        public string Metric { get; private set; }

        public InstrumentationTarget(MethodInfo method, string metricName, string metric)
        {
            this.Method = method;
            this.Metric = metric;
            this.MetricName = metricName;
        }
    }
}

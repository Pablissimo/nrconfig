using NewRelicConfiguration;
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
        public bool IsMethod { get; private set; }
        public bool IsConstructor { get; private set; }

        public MethodBase Method { get; private set; }
        public string MetricName { get; private set; }
        public Metric Metric { get; private set; }

        public InstrumentationTarget(MethodInfo method, string metricName, Metric metric)
        {
            this.Method = method;
            this.MetricName = metricName;
            this.Metric = metric;

            this.IsMethod = true;
        }

        public InstrumentationTarget(ConstructorInfo constructor, string metricName, Metric metric)
        {
            this.Method = constructor;
            this.MetricName = metricName;
            this.Metric = metric;

            this.IsConstructor = true;
        }
    }
}

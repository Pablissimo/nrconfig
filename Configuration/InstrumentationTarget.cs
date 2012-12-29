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
        public bool IsMethod { get { return this.Method != null; } }
        public bool IsConstructor { get { return this.Constructor != null; } }

        public MethodInfo Method { get; private set; }
        public ConstructorInfo Constructor { get; private set; }
        public string MetricName { get; private set; }
        public Metric Metric { get; private set; }

        public InstrumentationTarget(MethodInfo method, string metricName, Metric metric)
        {
            this.Method = method;
            this.MetricName = metricName;
            this.Metric = metric;
        }

        public InstrumentationTarget(ConstructorInfo constructor, string metricName, Metric metric)
        {
            this.Constructor = constructor;
            this.MetricName = metricName;
            this.Metric = metric;
        }
    }
}

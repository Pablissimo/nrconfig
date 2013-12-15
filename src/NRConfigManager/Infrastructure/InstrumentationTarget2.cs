using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public class InstrumentationTarget2
    {
        public IMethodDetails Target { get; private set; }
        public string MetricName { get; private set; }
        public Metric Metric { get; private set; }

        public InstrumentationTarget2(IMethodDetails target, string metricName, Metric metric)
        {
            this.Target = target;
            this.MetricName = metricName;
            this.Metric = metric;
        }
    }
}

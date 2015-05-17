using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public class InstrumentationTarget
    {
        public IMethodDetails Target { get; private set; }
        public string MetricName { get; private set; }
        public string Name { get; set; }
        public string TransactionNamingPriority { get; set; }
        public Metric Metric { get; private set; }

        public InstrumentationTarget(IMethodDetails target, string metricName, string name, string transactionNamingPriority, Metric metric)
        {
            this.Target = target;
            this.MetricName = metricName;
            this.Name = name;
            this.TransactionNamingPriority = transactionNamingPriority;
            this.Metric = metric;
        }
    }
}

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
        public string Metric { get; set; }
    }
}

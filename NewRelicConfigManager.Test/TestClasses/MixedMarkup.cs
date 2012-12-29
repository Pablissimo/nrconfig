using NewRelicConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Test.TestClasses
{
    [Instrument("Metric name", "Metric")]
    public class MixedMarkup
    {
        [Instrument("Method-level override name")]
        public void TestMethodDifferentMetricName()
        {

        }

        [Instrument(Metric = "Method-level override metric")]
        public void TestMethodDifferentMetricInheritedMetricName()
        {

        }

        [Instrument("Method-level override name", "Method-level override metric")]
        public void TestMethodDifferentMetricAndMetricName()
        {

        }

        [Instrument("Override class-level metric name")]
        public class Nested
        {
            public void OverrideMetricNameInNestedClassByDefault()
            {

            }
        }
    }
}

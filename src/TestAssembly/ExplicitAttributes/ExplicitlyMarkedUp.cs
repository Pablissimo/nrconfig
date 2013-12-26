using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.ExplicitAttributes
{
    [Instrument(Metric = Metric.Scoped, MetricName = "My metric name", Scopes = InstrumentationScopes.All)]
    public class ExplicitlyMarkedUp
    {
        public void PublicMethod()
        {
        }

        protected void ProtectedMethod()
        {
        }

        private void PrivateMethod()
        {
        }

        private class NestedClass
        {
            public void PublicMethodInPrivateNestedClass()
            {
            }

            [Instrument(MetricName = "My second metric name")]
            public void MethodWithDifferentMetricName()
            {
            }
        }
    }
}

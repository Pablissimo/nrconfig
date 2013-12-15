using NRConfig;
using NRConfigManager.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Configuration
{
    /// <summary>
    /// Describes a single method (including constructors and property accessors) 
    /// that is to be instrumented by NewRelic.
    /// </summary>
    public class InstrumentationTarget
    {
        /// <summary>
        /// Gets whether the target of instrumentation is a method or
        /// property accessor.
        /// </summary>
        public bool IsMethodOrPropertyAccessor { get; private set; }

        /// <summary>
        /// Gets whether the target of the instrumentation is a constructor.
        /// </summary>
        public bool IsConstructor { get; private set; }

        /// <summary>
        /// Gets the target to be instrumented.
        /// </summary>
        public MethodBase Target { get; private set; }
        /// <summary>
        /// Gets the name of the metric against which telemetry should be
        /// recorded.
        /// </summary>
        public string MetricName { get; private set; }
        /// <summary>
        /// Gets the metric against which telemetry should be recorded.
        /// </summary>
        public Metric Metric { get; private set; }

        public InstrumentationTarget(MethodInfo method, string metricName, Metric metric)
        {
            this.Target = method;
            this.MetricName = metricName;
            this.Metric = metric;

            this.IsMethodOrPropertyAccessor = true;
        }

        public InstrumentationTarget(ConstructorInfo constructor, string metricName, Metric metric)
        {
            this.Target = constructor;
            this.MetricName = metricName;
            this.Metric = metric;

            this.IsConstructor = true;
        }

        public InstrumentationTarget(IMethodDetails method, string metricName, Metric metric)
        {
        }

        public InstrumentationTarget(IConstructorDetails ctor, string metricName, Metric metric)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NewRelicConfiguration
{
    /// <summary>
    /// Specifies that an assembly, class, method, property or constructor should be instrumented
    /// by New Relic.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Assembly)]
    public class InstrumentAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the metric against which telemetry should be recorded.
        /// </summary>
        public string MetricName { get; set; }
        /// <summary>
        /// Gets or sets the metric against which telemetry should be recorded.
        /// </summary>
        public Metric Metric { get; set; }
        /// <summary>
        /// Gets or sets the combination of flags indicating how the search for instrumentation targets
        /// should proceed from this point forward.
        /// </summary>
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

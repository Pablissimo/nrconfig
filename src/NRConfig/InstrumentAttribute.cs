using System;
using System.Collections.Generic;
using System.Text;

namespace NRConfig
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
        /// Get or set factory instance name for tracer
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Get or set transaction naming priority (7 takes precendence over 1 or 6)
        /// </summary>
        public string TransactionNamingPriority { get; set; }
        /// <summary>
        /// Gets or sets the metric against which telemetry should be recorded.
        /// </summary>
        public Metric Metric { get; set; }
        /// <summary>
        /// Gets or sets the combination of flags indicating how the search for instrumentation targets
        /// should proceed from this point forward.
        /// </summary>
        public InstrumentationScopes Scopes { get; set; }
        /// <summary>
        /// Gets or sets whether classes or methods marked with the 'CompilerGenerated' attribute
        /// should be included. If not specified the value is inherited.
        /// </summary>
        private bool _includeCompilerGeneratedCode;
        public bool IncludeCompilerGeneratedCode
        {
            get
            {
                return _includeCompilerGeneratedCode;
            }
            set
            {
                _includeCompilerGeneratedCode = value;
                this.IncludeCompilerGeneratedCodeSet = true;
            }
        }

        public bool IncludeCompilerGeneratedCodeSet { get; private set; }

        public InstrumentAttribute()
            : this(null) {}

        public InstrumentAttribute(string metricName)
            : this(metricName, Metric.Unspecified) {}

        public InstrumentAttribute(string metricName, Metric metric)
            :this(metricName, null, metric) {}
        
        public InstrumentAttribute(string metricName, string name, Metric metric)
            :this(metricName, null, null, metric) {}

        public InstrumentAttribute(string metricName, string name, string transactionNamingPriority, Metric metric)
        {
            this.Scopes = InstrumentationScopes.All;

            this.Name = name;
            this.TransactionNamingPriority = transactionNamingPriority;
            this.MetricName = metricName;
            this.Metric = metric;
        }

        /// <summary>
        /// Generates an instrumentation context that represents the configuration supplied by an ordered
        /// collection of contexts, where contexts earlier in the list take higher precedence.
        /// </summary>
        /// <param name="attrs">Zero or more InstrumentAttribute objects representing full or partial
        /// contexts to be combined.</param>
        /// <returns>An InstrumentAttribute representing the context that best describes the supplied
        /// hierarchy of full or partial contexts.</returns>
        public static InstrumentAttribute GetEffectiveInstrumentationContext(params InstrumentAttribute[] attrs)
        {
            // Working through the array, assuming that the top-most items are the most important
            InstrumentAttribute toReturn = new InstrumentAttribute();
            bool setMetricName = false, setName = false, setTransactionNamingPriority = false, setMetric = false, setScopes = false, setIncludeCompilerGenerated = false;

            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    if (attr == null)
                    {
                        continue;
                    }
                    else if (setMetricName && setName && setTransactionNamingPriority && setMetric && setIncludeCompilerGenerated && setScopes)
                    {
                        break;
                    }

                    if (attr.MetricName != null && !setMetricName)
                    {
                        toReturn.MetricName = attr.MetricName;
                        setMetricName = true;
                    }

                    if (attr.Name != null && !setName)
                    {
                        toReturn.Name = attr.Name;
                        setName = true;
                    }
                    
                    if (attr.TransactionNamingPriority != null && !setTransactionNamingPriority)
                    {
                        toReturn.TransactionNamingPriority = attr.TransactionNamingPriority;
                        setTransactionNamingPriority = true;
                    }

                    if (attr.Metric != Metric.Unspecified && !setMetric)
                    {
                        toReturn.Metric = attr.Metric;
                        setMetric = true;
                    }

                    if (!setScopes)
                    {
                        toReturn.Scopes = attr.Scopes;
                        setScopes = true;
                    }

                    if (attr.IncludeCompilerGeneratedCodeSet && !setIncludeCompilerGenerated)
                    {
                        toReturn.IncludeCompilerGeneratedCode = attr.IncludeCompilerGeneratedCode;
                        setIncludeCompilerGenerated = true;
                    }
                }
            }

            bool anyNonNullAttrs = false;
            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    if (attr != null)
                    {
                        anyNonNullAttrs = true;
                        break;
                    }
                }
            }

            if (!anyNonNullAttrs)
            {
                toReturn = null;
            }

            return toReturn;
        }
    }
}

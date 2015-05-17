using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NRConfigManager.Rendering
{
    /// <summary>
    /// Class for New Relic-compatible XML output of the extension element
    /// </summary>
    [XmlRoot(ElementName = "extension", Namespace = "urn:newrelic-extension")]
    public class Extension
    {
        [XmlElement(ElementName="instrumentation")]
        public Instrumentation Instrumentation { get; set; }

        public Extension()
        {
            this.Instrumentation = new Instrumentation();
        }

        /// <summary>
        /// Produces a new Extension object that represents the union of all instrumentation
        /// targets specified in the supplied Extension objects.
        /// </summary>
        /// <param name="toMerge">The Extension objects to be merged.</param>
        /// <returns>A single Extension object that represents the combined instrumentation
        /// footprint described by all of the supplied Extension objects.</returns>
        public static Extension Merge(params Extension[] toMerge)
        {
            Extension toReturn = new Extension();

            var matchRecords = new Dictionary<DenormalisedExactMatchRecord, HashSet<ExactMethodMatcher>>();

            foreach (var factory in toMerge.SelectMany(x => x.Instrumentation.TracerFactories))
            {
                foreach (var match in factory.MatchDefinitions)
                {
                    DenormalisedExactMatchRecord matchRecord = new DenormalisedExactMatchRecord
                    {
                        Metric = factory.Metric,
                        MetricName = factory.MetricName,
                        Name = factory.Name,
                        TransactionNamingPriority = factory.TransactionNamingPriority,
                        AssemblyName = match.AssemblyName,
                        ClassName = match.ClassName
                    };

                    HashSet<ExactMethodMatcher> matchers = null;
                    
                    if (!matchRecords.TryGetValue(matchRecord, out matchers))
                    {
                        matchers = matchRecords[matchRecord] = new HashSet<ExactMethodMatcher>();
                    }

                    matchers.UnionWith(match.Matches.Select(x => new ExactMethodMatcher { MethodName = x.MethodName, ParameterTypes = x.ParameterTypes }));
                }
            }

            // Group the records by factory details, then by assy/classname pair
            var keysByFactoryDetails = matchRecords.Keys.GroupBy(x => new { Metric = x.Metric, MetricName = x.MetricName, Name = x.Name, TransactionNamingPriority = x.TransactionNamingPriority});
            foreach (var factoryDetail in keysByFactoryDetails)
            {
                TracerFactory toAdd = new TracerFactory(factoryDetail.Key.MetricName, factoryDetail.Key.Name, factoryDetail.Key.TransactionNamingPriority, factoryDetail.Key.Metric);
                
                var byClassDetail = factoryDetail.GroupBy(x => new { AssemblyName = x.AssemblyName, ClassName = x.ClassName });
                foreach (var classDetail in byClassDetail)
                {
                    Match matchToAdd = new Match(classDetail.Key.AssemblyName, classDetail.Key.ClassName);

                    var matchRecord = new DenormalisedExactMatchRecord
                    {
                        Metric = toAdd.Metric,
                        MetricName = toAdd.MetricName,
                        Name = toAdd.Name,
                        TransactionNamingPriority = toAdd.TransactionNamingPriority,
                        AssemblyName = matchToAdd.AssemblyName,
                        ClassName = matchToAdd.ClassName
                    };

                    matchToAdd.Matches = matchRecords[matchRecord].OrderBy(x => x.MethodName).ThenBy(x => x.ParameterTypes).ToList();
                    toAdd.MatchDefinitions.Add(matchToAdd);
                }

                toAdd.MatchDefinitions = toAdd.MatchDefinitions.OrderBy(x => x.AssemblyName).ThenBy(x => x.ClassName).ToList();

                toReturn.Instrumentation.TracerFactories.Add(toAdd);
            }

            // Have tracer factories prefer unspecified metrics and metric names first, then others later
            toReturn.Instrumentation.TracerFactories =
                toReturn
                .Instrumentation
                .TracerFactories
                .OrderBy(x => x.Metric == Metric.Unspecified ? -1 : (int) x.Metric)
                .ThenBy(x => x.MetricName ?? string.Empty)
                .ThenBy(x => x.Name ?? string.Empty)
                .ThenBy(x => x.TransactionNamingPriority ?? string.Empty)
                .ToList();

            return toReturn;
        }

        class DenormalisedExactMatchRecord
        {
            public Metric Metric { get; set; }
            public string MetricName { get; set; }
            public string Name { get; set; }
            public string TransactionNamingPriority { get; set; }
            public string AssemblyName { get; set; }
            public string ClassName { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as DenormalisedExactMatchRecord;
                if (other == null)
                {
                    return false;
                }
                else
                {
                    return
                        this.Metric == other.Metric
                        && this.MetricName == other.MetricName
                        && this.Name == other.Name
                        && this.TransactionNamingPriority == other.TransactionNamingPriority
                        && this.AssemblyName == other.AssemblyName
                        && this.ClassName == other.ClassName;
                }
            }

            public override int GetHashCode()
            {
                return
                    this.Metric.GetHashCode() + 
                    (251 * (this.MetricName ?? "").GetHashCode() + 
                    (251 * (this.Name ?? "").GetHashCode() + 
                    (251 * (this.AssemblyName ?? "").GetHashCode() + 
                    (251 * (this.ClassName ?? "").GetHashCode()))));
            }
        }
    }
}

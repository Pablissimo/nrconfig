using NewRelicConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewRelicConfigManager.Rendering
{
    [XmlRoot(ElementName = "extension", Namespace = "urn:newrelic-extension")]
    public class Extension
    {
        [XmlElement(ElementName="instrumentation")]
        public Instrumentation Instrumentation { get; set; }

        public Extension()
        {
            this.Instrumentation = new Instrumentation();
        }

        public static Extension Merge(params Extension[] toMerge)
        {
            Extension toReturn = new Extension();

            var matchRecords = new Dictionary<DenormalisedExactMatchRecord, List<ExactMethodMatcher>>();

            foreach (var factory in toMerge.SelectMany(x => x.Instrumentation.TracerFactories))
            {
                foreach (var match in factory.MatchDefinitions)
                {
                    DenormalisedExactMatchRecord matchRecord = new DenormalisedExactMatchRecord
                    {
                        Metric = factory.Metric,
                        MetricName = factory.MetricName,
                        AssemblyName = match.AssemblyName,
                        ClassName = match.ClassName
                    };

                    List<ExactMethodMatcher> matchers = null;
                    
                    if (!matchRecords.TryGetValue(matchRecord, out matchers))
                    {
                        matchers = matchRecords[matchRecord] = new List<ExactMethodMatcher>();
                    }

                    matchers.AddRange(match.Matches.Select(x => new ExactMethodMatcher { MethodName = x.MethodName, ParameterTypes = x.ParameterTypes }));
                }
            }

            // Group the records by factory details, then by assy/classname pair
            var keysByFactoryDetails = matchRecords.Keys.GroupBy(x => new { Metric = x.Metric, MetricName = x.MetricName });
            foreach (var factoryDetail in keysByFactoryDetails)
            {
                TracerFactory toAdd = new TracerFactory(factoryDetail.Key.MetricName, factoryDetail.Key.Metric);
                
                var byClassDetail = factoryDetail.GroupBy(x => new { AssemblyName = x.AssemblyName, ClassName = x.ClassName });
                foreach (var classDetail in byClassDetail)
                {
                    Match matchToAdd = new Match(classDetail.Key.AssemblyName, classDetail.Key.ClassName);

                    var matchRecord = new DenormalisedExactMatchRecord
                    {
                        Metric = toAdd.Metric,
                        MetricName = toAdd.MetricName,
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
                .ToList();

            return toReturn;
        }

        class DenormalisedExactMatchRecord
        {
            public Metric Metric { get; set; }
            public string MetricName { get; set; }
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
                        && this.AssemblyName == other.AssemblyName
                        && this.ClassName == other.ClassName;
                }
            }

            public override int GetHashCode()
            {
                return
                    this.Metric.GetHashCode() + 
                    (251 * (this.MetricName ?? "").GetHashCode() + 
                    (251 * (this.AssemblyName ?? "").GetHashCode() + 
                    (251 * (this.ClassName ?? "").GetHashCode())));
            }
        }
    }
}

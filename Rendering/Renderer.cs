using NewRelicConfigManager.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NewRelicConfigManager.Rendering
{
    public class Renderer
    {
        public Extension Render(IEnumerable<InstrumentationTarget> targets)
        {
            Instrumentation toReturn = new Instrumentation();

            // Group the targets by metric name...
            var byMetricName = targets.GroupBy(x => x.MetricName ?? string.Empty);
            foreach (var groupedByMetricName in byMetricName.OrderBy(x => x.Key))
            {
                TracerFactory tracerFactory = new TracerFactory(groupedByMetricName.Key);

                // Then group by metric
                var byMetric = groupedByMetricName.GroupBy(x => x.Metric ?? string.Empty);
                foreach (var groupedByMetric in byMetric.OrderBy(x => x.Key))
                {
                    Func<InstrumentationTarget, Type> typeGetter = x => x.IsConstructor ? x.Constructor.DeclaringType : x.Method.DeclaringType;
                    var byType = groupedByMetric.GroupBy(x => typeGetter(x));
                    foreach (var groupedByType in byType)
                    {
                        Match match = GetMatchFromType(groupedByType.Key, groupedByMetric.Key);

                        // Each item in the groupedByType enumerable is a method to be instrumented
                        foreach (var toInstrument in groupedByType)
                        {
                            ExactMethodMatcher methodMatcher = GetMatcherFromTarget(toInstrument);
                            match.Matches.Add(methodMatcher);
                        }

                        tracerFactory.MatchDefinitions.Add(match);
                    }
                }

                toReturn.TracerFactories.Add(tracerFactory);
            }

            return new Extension() { Instrumentation = toReturn };
        }

        public void RenderToStream(IEnumerable<InstrumentationTarget> targets, Stream stream)
        {
            Extension rootElement = Render(targets);

            XmlSerializer serializer = new XmlSerializer(typeof(Extension));
            serializer.Serialize(stream, rootElement);
        }

        private ExactMethodMatcher GetMatcherFromTarget(InstrumentationTarget target)
        {
            string methodName = target.IsConstructor ? target.Constructor.Name : target.Method.Name;
            ParameterInfo[] parameters = target.IsConstructor ? target.Constructor.GetParameters() : target.Method.GetParameters();

            string[] parameterTypeNames = (parameters ?? Enumerable.Empty<ParameterInfo>()).Select(x => x.ParameterType.FullName).ToArray();

            if (parameters == null || !parameters.Any())
            {
                parameterTypeNames = new[]{ "void" };
            }

            return new ExactMethodMatcher(methodName, parameterTypeNames.ToArray());
        }

        private Match GetMatchFromType(Type t, string metric)
        {
            var assy = t.Assembly.GetName().Name;
            var className = t.ToString();

            return new Match(metric, assy, className);
        }
    }
}

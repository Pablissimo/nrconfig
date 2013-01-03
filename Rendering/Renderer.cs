using NewRelicConfigManager.Configuration;
using NewRelicConfiguration;
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
                // Then group by metric
                var byMetric = groupedByMetricName.GroupBy(x => x.Metric);
                foreach (var groupedByMetric in byMetric.OrderBy(x => x.Key))
                {
                    string metricName = groupedByMetricName.Key;
                    if (metricName == string.Empty)
                    {
                        metricName = null;
                    }

                    TracerFactory tracerFactory = new TracerFactory(metricName, groupedByMetric.Key);

                    Func<InstrumentationTarget, Type> typeGetter = x => x.Method.DeclaringType;
                    var byType = groupedByMetric.GroupBy(x => typeGetter(x));
                    foreach (var groupedByType in byType)
                    {
                        Match match = GetMatchFromType(groupedByType.Key);

                        // Each item in the groupedByType enumerable is a method to be instrumented
                        foreach (var toInstrument in groupedByType)
                        {
                            ExactMethodMatcher methodMatcher = GetMatcherFromTarget(toInstrument);
                            match.Matches.Add(methodMatcher);
                        }

                        tracerFactory.MatchDefinitions.Add(match);
                    }

                    toReturn.TracerFactories.Add(tracerFactory);
                }
            }

            return new Extension() { Instrumentation = toReturn };
        }

        public void RenderToStream(IEnumerable<InstrumentationTarget> targets, Stream stream)
        {
            Extension rootElement = Render(targets);

            XmlSerializer serializer = new XmlSerializer(typeof(Extension));
            serializer.Serialize(stream, rootElement);
        }
        
        public void RenderToStream(Extension extension, Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Extension));
            serializer.Serialize(stream, extension);
        }

        public Extension LoadRenderedFromStream(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Extension));
            return serializer.Deserialize(stream) as Extension;
        }

        private ExactMethodMatcher GetMatcherFromTarget(InstrumentationTarget target)
        {
            string methodName = target.Method.Name;
            ParameterInfo[] parameters = target.Method.GetParameters();

            string[] parameterTypeNames = (parameters ?? Enumerable.Empty<ParameterInfo>()).Select(x => GetFriendlyTypeName(x.ParameterType)).ToArray();

            if (parameters == null || !parameters.Any())
            {
                parameterTypeNames = new[]{ "void" };
            }

            return new ExactMethodMatcher(methodName, parameterTypeNames.ToArray());
        }

        private static string GetFriendlyTypeName(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.FullName;
            }
            else
            {
                // Generics when asked for their full-name do crazy things like:
                // System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                // Which isn't much use to us as NewRelic uses a combination of backtick notation and angle brackets. 
                // However, we can fiddle this...
                const string FORMAT = "{0}.{1}`{2}<{3}>";
                string[] innerTypes = t.GenericTypeArguments.Select(x => string.Format("{0}", GetFriendlyTypeName(x))).ToArray();

                return string.Format(FORMAT, t.Namespace, t.Name, t.GenericTypeArguments.Count(), string.Join(",", innerTypes));
            }
        }

        private Match GetMatchFromType(Type t)
        {
            var assy = t.Assembly.GetName().Name;
            var className = t.ToString();

            return new Match(assy, className);
        }
    }
}

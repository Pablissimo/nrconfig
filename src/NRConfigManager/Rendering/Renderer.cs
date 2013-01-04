using NRConfigManager.Configuration;
using NRConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NRConfigManager.Rendering
{
    /// <summary>
    /// Renders or manipulates New Relic-compatible XML configuration files for custom instrumentation.
    /// </summary>
    public static class Renderer
    {
        /// <summary>
        /// Creates an XML-renderable Extension object that represents the instrumentation settings
        /// specified by the supplied list of targets.
        /// </summary>
        /// <param name="targets">The set of targets to be instrumented.</param>
        /// <returns>An Extension object representing the in-memory New Relic custom
        /// instrumentation configuration XML document.</returns>
        public static Extension Render(IEnumerable<InstrumentationTarget> targets)
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

                    Func<InstrumentationTarget, Type> typeGetter = x => x.Target.DeclaringType;
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

        /// <summary>
        /// Renders a supplied collection of instrumentation targets to a specified stream as
        /// a New Relic-compatible XML document.
        /// </summary>
        /// <param name="targets">The set of targets to be instrumented.</param>
        /// <param name="stream">The stream to which the XML document should be rendered.</param>
        public static void RenderToStream(IEnumerable<InstrumentationTarget> targets, Stream stream)
        {
            Extension rootElement = Render(targets);

            XmlSerializer serializer = new XmlSerializer(typeof(Extension));
            serializer.Serialize(stream, rootElement);
        }
        
        /// <summary>
        /// Renders a supplied Extension object to a specified stream as a New Relic-compatible
        /// XML document.
        /// </summary>
        /// <param name="extension">The Extension object to be rendered.</param>
        /// <param name="stream">The stream to which the XML document should be rendered.</param>
        public static void RenderToStream(Extension extension, Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Extension));
            serializer.Serialize(stream, extension);
        }

        /// <summary>
        /// Generates an in-memory representation of a New Relic custom instrumentation
        /// XML file from the specified stream.
        /// </summary>
        /// <param name="stream">The stream from which the document should be loaded.</param>
        /// <returns>An Extension object representing the contents of the stream.</returns>
        public static Extension LoadRenderedFromStream(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Extension));
            return serializer.Deserialize(stream) as Extension;
        }

        internal static ExactMethodMatcher GetMatcherFromTarget(InstrumentationTarget target)
        {
            string methodName = target.Target.Name;
            ParameterInfo[] parameters = target.Target.GetParameters();

            string[] parameterTypeNames = null;

            if (target.Target.ContainsGenericParameters)
            {
                var method = target.Target;
                var genericArgs = method.GetGenericArguments();

                // Match up the generic arguments to the parameter types as required
                List<string> tempParamTypeNames = new List<string>();
                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType.IsGenericParameter)
                    {
                        int matchingIdx = -1;
                        for (int i = 0; i < genericArgs.Length; i++)
                        {
                            var t = genericArgs[i];

                            if (t.Name == parameter.ParameterType.Name)
                            {
                                matchingIdx = i;
                            }
                        }

                        if (matchingIdx > -1)
                        {
                            tempParamTypeNames.Add(string.Format("<MVAR {0}>", matchingIdx));
                        }
                        else
                        {
                            throw new InvalidOperationException("Can't find the matching generic type parameter for argument " + parameter);
                        }
                    }
                    else
                    {
                        tempParamTypeNames.Add(GetFriendlyTypeName(parameter.ParameterType));
                    }
                }

                parameterTypeNames = tempParamTypeNames.ToArray();
            }
            else
            {
                parameterTypeNames = (parameters ?? Enumerable.Empty<ParameterInfo>()).Select(x => GetFriendlyTypeName(x.ParameterType)).ToArray();
            }            

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
                const string FORMAT = "{0}.{1}<{2}>";
                string[] innerTypes = t.GenericTypeArguments.Select(x => string.Format("{0}", GetFriendlyTypeName(x))).ToArray();

                return string.Format(FORMAT, t.Namespace, t.Name, string.Join(",", innerTypes));
            }
        }

        private static Match GetMatchFromType(Type t)
        {
            var assy = t.Assembly.GetName().Name;
            var className = t.ToString();

            return new Match(assy, className);
        }
    }
}

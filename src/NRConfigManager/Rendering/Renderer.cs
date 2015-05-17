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
using NRConfigManager.Infrastructure;

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
                // Then by name
                var byName = groupedByMetricName.GroupBy(x => x.Name ?? string.Empty);
                foreach (var groupedByName in byName.OrderBy(x => x.Key))
                {
                    // Then by priority
                    var byPriority = groupedByName.GroupBy(x => x.TransactionNamingPriority ?? string.Empty);
                    foreach (var groupedByPriority in byPriority.OrderBy(x => x.Key))
                    {
                        // Then group by metric
                        var byMetric = groupedByPriority.GroupBy(x => x.Metric);
                        foreach (var groupedByMetric in byMetric.OrderBy(x => x.Key))
                        {
                            string metricName = groupedByMetricName.Key == string.Empty ? null : groupedByMetricName.Key;
                            string name = groupedByName.Key == string.Empty ? null : groupedByName.Key;
                            string priority = groupedByPriority.Key == string.Empty ? null : groupedByPriority.Key;

                            TracerFactory tracerFactory = new TracerFactory(metricName, name, priority, groupedByMetric.Key);

                            var byType = groupedByMetric.GroupBy(x => x.Target.DeclaringType);
                            foreach (var groupedByType in byType.OrderBy(x => x.Key.Assembly.FullName).ThenBy(x => x.Key.FullName))
                            {
                                Match match = GetMatchFromType(groupedByType.Key);

                                // Each item in the groupedByType enumerable is a method to be instrumented
                                foreach (var toInstrument in groupedByType.OrderBy(x => x.Target.Name))
                                {
                                    ExactMethodMatcher methodMatcher = GetMatcherFromTarget(toInstrument);
                                    match.Matches.Add(methodMatcher);
                                }

                                // De-dupe the method matchers, in case we have some parameterless
                                // entries and some with - as the parameterless ones will take precedence
                                // anyway, there's no point keeping the others
                                HashSet<ExactMethodMatcher> toDelete = new HashSet<ExactMethodMatcher>();
                                foreach (var matcher in match.Matches.Where(x => string.IsNullOrWhiteSpace(x.ParameterTypes)))
                                {
                                    toDelete.UnionWith(match.Matches.Where(x => x.MethodName == matcher.MethodName && !string.IsNullOrWhiteSpace(x.ParameterTypes)));
                                }

                                match.Matches.RemoveAll(x => toDelete.Contains(x));

                                tracerFactory.MatchDefinitions.Add(match);
                            }

                            toReturn.TracerFactories.Add(tracerFactory);
                        }
                    }
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
            var parameters = target.Target.Parameters;

            string[] parameterTypeNames = null;

            if (target.Target.ContainsGenericParameters)
            {
                var method = target.Target;
                var genericArgs = method.GenericArguments;

                // Match up the generic arguments to the parameter types as required
                List<string> tempParamTypeNames = new List<string>();
                foreach (var parameter in parameters)
                {
                    if (parameter.Type.IsGenericType)
                    {
                        tempParamTypeNames.Add(GetGenericParameterTypeName(parameter.Type, genericArgs));
                    }
                    else if (parameter.Type.IsGenericParameter)
                    {
                        tempParamTypeNames.Add(string.Format("<{0}>", GetGenericParameterTypeName(parameter.Type, genericArgs)));
                    }
                    else
                    {
                        tempParamTypeNames.Add(GetFriendlyTypeName(parameter.Type));
                    }
                }

                parameterTypeNames = tempParamTypeNames.ToArray();
            }
            else
            {
                parameterTypeNames = (parameters ?? Enumerable.Empty<IParameterDetails>()).Select(x => GetFriendlyTypeName(x.Type)).ToArray();
            }            

            // This is a kludge to compensate for a problem specifying parameters in the instrumentation file
            // where a parameter is a closed generic with more than one generic type parameter - for example,
            // Dictionary<string, int> or KeyValuePair<string, string>. To get around the problem, if we detect
            // such a method, we'll just output a parameterless matcher definition. This has the negative side-effect
            // that we'll inadvertently instrument all overloads of the method (if any exist), even if they don't
            // match our instrumentation criteria
            if (parameters != null && parameters.Any(x => x.Type.IsGenericType && x.Type.GenericArguments.Count() > 1))
            {
                parameterTypeNames = new string[0];
            }
            else if (parameters == null || !parameters.Any())
            {
                parameterTypeNames = new[]{ "void" };
            }

            return new ExactMethodMatcher(methodName, parameterTypeNames.ToArray());
        }

        private static string GetGenericParameterTypeName(ITypeDetails parameterType, IEnumerable<ITypeDetails> orderedTypeContext)
        {
            if (parameterType.IsGenericType)
            {
                return parameterType.FullName + "<" + string.Join(",", parameterType.GenericArguments.Select(x => GetGenericParameterTypeName(x, orderedTypeContext))) + ">";
            }
            else
            {
                // Is this something that's in our context?
                int idx = 0;
                foreach (var genericArgument in orderedTypeContext)
                {
                    if (genericArgument.Equals(parameterType))
                    {
                        return string.Format("MVAR {0}", idx);
                    }

                    idx++;
                }
            }

            // Fall back to standard process
            return GetFriendlyTypeName(parameterType);
        }

        private static string GetFriendlyTypeName(ITypeDetails t)
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
                string[] innerTypes = t.GenericArguments.Select(x => string.Format("{0}", GetFriendlyTypeName(x))).ToArray();

                return string.Format(FORMAT, t.Namespace, t.Name, string.Join(",", innerTypes));
            }
        }

        private static Match GetMatchFromType(ITypeDetails t)
        {
            var assy = t.Assembly.Name;
            var className = t.FullName;

            return new Match(assy, className);
        }
    }
}

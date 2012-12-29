using NewRelicConfigManager.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Rendering
{
    public class Renderer
    {
        public Instrumentation Render(IEnumerable<InstrumentationTarget> targets)
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
                    var byType = groupedByMetric.GroupBy(x => x.Method.DeclaringType);
                    foreach (var groupedByType in byType)
                    {
                        Match match = GetMatchFromType(groupedByType.Key, groupedByMetric.Key);

                        // Each item in the groupedByType enumerable is a method to be instrumented
                        foreach (var toInstrument in groupedByType)
                        {
                            ExactMethodMatcher methodMatcher = GetMatcherFromTarget(toInstrument);
                            match.Matches.Add(methodMatcher);
                        }
                    }
                }

                toReturn.TracerFactories.Add(tracerFactory);
            }

            return toReturn;
        }

        private ExactMethodMatcher GetMatcherFromTarget(InstrumentationTarget target)
        {
            return new ExactMethodMatcher(target.Method.Name, (target.Method.GetParameters() ?? Enumerable.Empty<ParameterInfo>()).Select(x => x.ParameterType.FullName).ToArray());
        }

        private Match GetMatchFromType(Type t, string metric)
        {
            var assy = t.Assembly.FullName;
            var className = t.ToString();

            return new Match(metric, assy, className);
        }
    }
}

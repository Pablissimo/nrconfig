using NewRelicConfigManager.Configuration;
using NewRelicConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Infrastructure
{
    public static class InstrumentationDiscoverer
    {
        private static Type _instAttributeType = typeof(InstrumentAttribute);

        public static IEnumerable<InstrumentationTarget> GetInstrumentationSet(Assembly assy)
        {
            List<InstrumentationTarget> toReturn = new List<InstrumentationTarget>();

            var allTypes = assy.GetTypes();
            foreach (Type t in allTypes.Where(x => x.IsClass))
            {
                toReturn.AddRange(GetInstrumentationSet(t));
            }

            return toReturn;
        }

        public static IEnumerable<InstrumentationTarget> GetInstrumentationSet(Type t)
        {
            return GetInstrumentationSet(t, null);
        }

        private static IEnumerable<InstrumentationTarget> GetInstrumentationSet(Type t, InstrumentAttribute context)
        {
            List<InstrumentationTarget> toReturn = new List<InstrumentationTarget>();

            // Does the type have an Instrument attribute?
            var typeLevelAttribute = t.GetCustomAttribute(_instAttributeType) as InstrumentAttribute;
            if (typeLevelAttribute == null)
            {
                typeLevelAttribute = context;
            }

            if (typeLevelAttribute != null)
            {
                // Instrument everything in this type, irrespective of its member-level
                // details
                foreach (MethodInfo methodInfo in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    toReturn.Add(GetInstrumentationTarget(methodInfo, typeLevelAttribute));
                }

                foreach (PropertyInfo propertyInfo in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    toReturn.Add(GetInstrumentationTarget(propertyInfo.GetGetMethod(true), typeLevelAttribute));
                    toReturn.Add(GetInstrumentationTarget(propertyInfo.GetSetMethod(true), typeLevelAttribute));
                }
            }

            var nested = t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (nested != null && nested.Any())
            {
                foreach (var nestedType in nested)
                {
                    toReturn.AddRange(GetInstrumentationSet(nestedType, typeLevelAttribute));
                }
            }

            return toReturn;
        }

        private static InstrumentationTarget GetInstrumentationTarget(MethodInfo methodInfo, InstrumentAttribute context)
        {
            return new InstrumentationTarget(methodInfo, context.MetricName, context.Metric);
        }
    }
}

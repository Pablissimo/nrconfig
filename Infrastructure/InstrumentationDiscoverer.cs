using log4net;
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
        private static ILog _logger = LogManager.GetLogger(typeof(InstrumentationDiscoverer));

        private static Type _instAttributeType = typeof(InstrumentAttribute);

        public static IEnumerable<InstrumentationTarget> GetInstrumentationSet(IEnumerable<Assembly> assemblies)
        {
            List<InstrumentationTarget> toReturn = new List<InstrumentationTarget>();

            foreach (Assembly assy in assemblies)
            {
                toReturn.AddRange(GetInstrumentationSet(assy));
            }

            return toReturn;
        }

        public static IEnumerable<InstrumentationTarget> GetInstrumentationSet(Assembly assy)
        {
            return GetInstrumentationSet(assy, null);
        }

        public static IEnumerable<InstrumentationTarget> GetInstrumentationSet(Assembly assy, InstrumentAttribute context)
        {
            List<InstrumentationTarget> toReturn = new List<InstrumentationTarget>();

            _logger.InfoFormat("Processing assembly {0}", assy.FullName);

            var allTypes = assy.GetTypes();
            _logger.DebugFormat("Found {0} types in assembly {1}", allTypes.Count(), assy.FullName);

            foreach (Type t in allTypes.Where(x => x.IsClass))
            {
                toReturn.AddRange(GetInstrumentationSet(t, context));
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

            if (!t.IsGenericTypeDefinition)
            {
                _logger.DebugFormat("Processing type {0}", t.FullName);

                HashSet<MethodBase> alreadyAdded = new HashSet<MethodBase>();

                // Does the type have an Instrument attribute?
                var typeLevelAttribute = t.GetCustomAttribute(_instAttributeType) as InstrumentAttribute;
                typeLevelAttribute = GetEffectiveInstrumentationContext(typeLevelAttribute, context);

                var baseBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var propBindingFlags = baseBindingFlags;
                var methodBindingFlags = baseBindingFlags;
                var ctorBindingFlags = baseBindingFlags;

                if (typeLevelAttribute == null)
                {
                    propBindingFlags |= BindingFlags.NonPublic | BindingFlags.Public;
                    methodBindingFlags |= BindingFlags.NonPublic | BindingFlags.Public;
                    ctorBindingFlags |= BindingFlags.NonPublic | BindingFlags.Public;
                }
                else
                {
                    if ((typeLevelAttribute.Scopes & InstrumentationScopes.PublicProperties) == InstrumentationScopes.PublicProperties)
                    {
                        propBindingFlags |= BindingFlags.Public;
                    }
                    if ((typeLevelAttribute.Scopes & InstrumentationScopes.NonPublicProperties) == InstrumentationScopes.NonPublicProperties)
                    {
                        propBindingFlags |= BindingFlags.NonPublic;
                    }

                    if ((typeLevelAttribute.Scopes & InstrumentationScopes.PublicMethods) == InstrumentationScopes.PublicMethods)
                    {
                        methodBindingFlags |= BindingFlags.Public;
                    }
                    if ((typeLevelAttribute.Scopes & InstrumentationScopes.NonPublicMethods) == InstrumentationScopes.NonPublicMethods)
                    {
                        methodBindingFlags |= BindingFlags.NonPublic;
                    }

                    if ((typeLevelAttribute.Scopes & InstrumentationScopes.PublicConstructors) == InstrumentationScopes.PublicConstructors)
                    {
                        ctorBindingFlags |= BindingFlags.Public;
                    }
                    if ((typeLevelAttribute.Scopes & InstrumentationScopes.NonPublicConstructors) == InstrumentationScopes.NonPublicConstructors)
                    {
                        ctorBindingFlags |= BindingFlags.NonPublic;
                    }
                }

                _logger.DebugFormat("Prop flags {0}, Ctor flags {1}, Method flags {2}", propBindingFlags, ctorBindingFlags, methodBindingFlags);

                // Instrument everything in this type, irrespective of its member-level
                // details
                foreach (MethodInfo methodInfo in t.GetMethods(methodBindingFlags).Where(x => !x.ContainsGenericParameters))
                {
                    var attr = GetEffectiveInstrumentationContext(methodInfo.GetCustomAttribute(_instAttributeType) as InstrumentAttribute, typeLevelAttribute);
                    if (attr != null && alreadyAdded.Add(methodInfo))
                    {
                        toReturn.Add(GetInstrumentationTarget(methodInfo, attr));
                    }
                }

                foreach (PropertyInfo propertyInfo in t.GetProperties(propBindingFlags))
                {
                    var getMethod = propertyInfo.GetGetMethod(true);
                    var setMethod = propertyInfo.GetSetMethod(true);

                    if (getMethod != null && getMethod.ContainsGenericParameters)
                    {
                        getMethod = null;
                    }
                    if (setMethod != null && setMethod.ContainsGenericParameters)
                    {
                        setMethod = null;
                    }

                    var propLevelAttribute = propertyInfo.GetCustomAttribute(_instAttributeType) as InstrumentAttribute;

                    if (getMethod != null)
                    {
                        var getMethodAttr = GetEffectiveInstrumentationContext(propLevelAttribute, getMethod.GetCustomAttribute(_instAttributeType) as InstrumentAttribute, typeLevelAttribute);
                        if (getMethodAttr != null && alreadyAdded.Add(getMethod))
                        {
                            toReturn.Add(GetInstrumentationTarget(getMethod, getMethodAttr));
                        }
                    }

                    if (setMethod != null)
                    {
                        var setMethodAttr = GetEffectiveInstrumentationContext(propLevelAttribute, setMethod.GetCustomAttribute(_instAttributeType) as InstrumentAttribute, typeLevelAttribute);
                        if (setMethodAttr != null && alreadyAdded.Add(setMethod))
                        {
                            toReturn.Add(GetInstrumentationTarget(setMethod, setMethodAttr));
                        }
                    }
                }

                foreach (ConstructorInfo constructorInfo in t.GetConstructors(ctorBindingFlags).Where(x => !x.ContainsGenericParameters))
                {
                    var attr = GetEffectiveInstrumentationContext(constructorInfo.GetCustomAttribute(_instAttributeType) as InstrumentAttribute, typeLevelAttribute);
                    if (attr != null && alreadyAdded.Add(constructorInfo))
                    {
                        toReturn.Add(GetInstrumentationTarget(constructorInfo, attr));
                    }
                }

                var nested = t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(x => !x.IsGenericTypeDefinition);
                if (nested != null && nested.Any())
                {
                    _logger.DebugFormat("Found {0} nested types within {1}", nested.Count(), t.FullName);

                    foreach (var nestedType in nested)
                    {
                        toReturn.AddRange(GetInstrumentationSet(nestedType, typeLevelAttribute));
                    }
                }
            }
            else
            {
                _logger.DebugFormat("Skipping type {0} - generic types not supported", t.FullName);
            }

            return toReturn;
        }

        private static InstrumentationTarget GetInstrumentationTarget(MethodInfo methodInfo, InstrumentAttribute context)
        {
            _logger.DebugFormat("Including method {0}.{1}({2})", methodInfo.DeclaringType.FullName, methodInfo.Name, GetParameterSignature(methodInfo.GetParameters()));
            
            return new InstrumentationTarget(methodInfo, context.MetricName, context.Metric);
        }

        private static InstrumentationTarget GetInstrumentationTarget(ConstructorInfo ctorInfo, InstrumentAttribute context)
        {
            _logger.DebugFormat("Including constructor {0}.{1}({2})", ctorInfo.DeclaringType.FullName, ctorInfo.Name, GetParameterSignature(ctorInfo.GetParameters()));

            return new InstrumentationTarget(ctorInfo, context.MetricName, context.Metric);
        }

        private static string GetParameterSignature(ParameterInfo[] parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                return string.Empty;
            }
            else
            {
                return string.Join(",", parameters.Select(x => x.ParameterType.FullName));
            }
        }

        private static InstrumentAttribute GetEffectiveInstrumentationContext(params InstrumentAttribute[] attrs)
        {
            // Working through the array, assuming that the top-most items are the most important
            InstrumentAttribute toReturn = new InstrumentAttribute();
            bool setMetricName = false, setMetric = false, setScopes = false;
            
            foreach (var attr in attrs)
            {
                if (attr == null)
                {
                    continue;
                }
                else if (setMetricName && setMetric)
                {
                    break;
                }

                if (attr.MetricName != null && !setMetricName)
                {
                    toReturn.MetricName = attr.MetricName;
                    setMetricName = true;
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
            }

            if (attrs == null || !attrs.Any(x => x != null))
            {
                toReturn = null;
            }

            return toReturn;
        }
    }
}

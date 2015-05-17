using log4net;
using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public abstract class InstrumentationDiscovererBase
    {
        protected ILog _logger = LogManager.GetLogger(typeof(InstrumentationDiscovererBase));

        protected abstract IEnumerable<ITypeDetails> GetTypes(string assemblyPath);
        
        public virtual IEnumerable<InstrumentationTarget> GetInstrumentationSet(string assemblyPath, InstrumentAttribute context, Predicate<ITypeDetails> typeFilter)
        {
            var toReturn = new List<InstrumentationTarget>();

            _logger.InfoFormat("Processing assembly {0}", assemblyPath);

            if (typeFilter == null)
            {
                typeFilter = x => true;
            }

            var allTypes = this.GetTypes(assemblyPath).Where(x => x.IsClass && !x.IsNested && typeFilter(x));
            _logger.DebugFormat("Found {0} types", allTypes.Count());

            InstrumentAttribute assyContext = null;

            if (allTypes.Any())
            {
                assyContext = allTypes.First().Assembly.InstrumentationContext;
            }

            foreach (var t in allTypes)
            {
                toReturn.AddRange(GetInstrumentationSet(t, InstrumentAttribute.GetEffectiveInstrumentationContext(assyContext, context)));
            }

            return toReturn;
        }

        protected virtual IEnumerable<InstrumentationTarget> GetInstrumentationSet(ITypeDetails t, InstrumentAttribute context)
        {
            var toReturn = new List<InstrumentationTarget>();

            if (!t.IsGenericTypeDefinition)
            {
                // Does the type have an Instrument attribute?
                var typeLevelAttribute = t.InstrumentationContext;
                typeLevelAttribute = InstrumentAttribute.GetEffectiveInstrumentationContext(typeLevelAttribute, context);

                if (t.IsCompilerGenerated && (typeLevelAttribute == null || !typeLevelAttribute.IncludeCompilerGeneratedCode))
                {
                    // Bail out early - we've found a compiler-generated method and haven't been told to include 'em
                    _logger.DebugFormat("Skipping type {0} - compiler-generated and configuration set to skip", t.FullName);

                    return toReturn;
                }

                _logger.DebugFormat("Processing type {0}", t.FullName);

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
                foreach (var methodDetails in t.GetMethods(methodBindingFlags))
                {
                    _logger.DebugFormat("Examining method {0}", methodDetails.ToString());

                    var attr = InstrumentAttribute.GetEffectiveInstrumentationContext(methodDetails.InstrumentationContext, typeLevelAttribute);
                    if (attr != null && (!methodDetails.IsCompilerGenerated || attr.IncludeCompilerGeneratedCode))
                    {
                        toReturn.Add(GetInstrumentationTarget(methodDetails, attr));
                    }
                }

                foreach (var propertyDetails in t.GetProperties(propBindingFlags))
                {
                    _logger.DebugFormat("Examining property {0}.{1}", t.FullName, propertyDetails.Name);

                    var getMethod = propertyDetails.GetMethod;
                    var setMethod = propertyDetails.SetMethod;

                    if (getMethod != null && getMethod.ContainsGenericParameters)
                    {
                        getMethod = null;
                    }

                    if (setMethod != null && setMethod.ContainsGenericParameters)
                    {
                        setMethod = null;
                    }

                    if (getMethod != null)
                    {
                        var getMethodAttr = InstrumentAttribute.GetEffectiveInstrumentationContext(propertyDetails.InstrumentationContext, getMethod.InstrumentationContext, typeLevelAttribute);
                        if (getMethodAttr != null)
                        {
                            toReturn.Add(GetInstrumentationTarget(getMethod, getMethodAttr));
                        }
                    }

                    if (setMethod != null)
                    {
                        var setMethodAttr = InstrumentAttribute.GetEffectiveInstrumentationContext(propertyDetails.InstrumentationContext, setMethod.InstrumentationContext, typeLevelAttribute);
                        if (setMethodAttr != null)
                        {
                            toReturn.Add(GetInstrumentationTarget(setMethod, setMethodAttr));
                        }
                    }
                }

                foreach (var constructorDetails in t.GetConstructors(ctorBindingFlags).Where(x => !x.ContainsGenericParameters))
                {
                    _logger.DebugFormat("Examining method {0}", constructorDetails.ToString());

                    var attr = InstrumentAttribute.GetEffectiveInstrumentationContext(constructorDetails.InstrumentationContext, typeLevelAttribute);
                    if (attr != null)
                    {
                        toReturn.Add(GetInstrumentationTarget(constructorDetails, attr));
                    }
                }
                                
                // Process nested types recursively, rather than enumerating them from the get-go so that we can apply
                // instrumentation scoping recursively too - the nested class will take on the instrumentation configuration
                // of the containing type, if any, or whatever's been passed down by initial settings
                foreach (var nested in t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    toReturn.AddRange(GetInstrumentationSet(nested, typeLevelAttribute));
                }
            }
            else
            {
                _logger.DebugFormat("Skipping type {0} - generic types not supported", t.FullName);
            }

            return toReturn;
        }

        protected virtual InstrumentationTarget GetInstrumentationTarget(IMethodDetails method, InstrumentAttribute context)
        {
            return new InstrumentationTarget(method, context.MetricName, context.Name, context.TransactionNamingPriority, context.Metric);
        }
    }
}

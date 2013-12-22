using log4net;
using Microsoft.Cci;
using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci
{
    public class CciInstrumentationDiscoverer : InstrumentationDiscovererBase
    {
        private static new ILog _logger = LogManager.GetLogger(typeof(CciInstrumentationDiscoverer));

        private InstrumentAttribute GetAttributeFromType(INamedTypeDefinition type)
        {
            var attributes = type.Attributes ?? Enumerable.Empty<ICustomAttribute>();
            var matchingAttribute = attributes.Where(x => TypeHelper.GetTypeName(x.Type).EndsWith("InstrumentAttribute")).FirstOrDefault();

            InstrumentAttribute toReturn = null;
            Type instrumentAttributeType = typeof(InstrumentAttribute);

            if (matchingAttribute != null)
            {
                toReturn = new InstrumentAttribute();

                foreach (var namedArgument in matchingAttribute.NamedArguments)
                {
                    var matchingProperty = instrumentAttributeType.GetProperty(namedArgument.ArgumentName.Value, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    if (matchingProperty != null)
                    {
                        matchingProperty.SetValue(toReturn, (namedArgument.ArgumentValue as IMetadataConstant).Value);
                    }
                }
            }

            return toReturn;
        }

        protected override IEnumerable<ITypeDetails> GetTypes(string assemblyPath)
        {
            var host = new PeReader.DefaultHost();
            var assy = host.LoadUnitFrom(assemblyPath) as IAssembly;

            if (assy == null || assy == Dummy.Assembly)
            {
                throw new InvalidOperationException(string.Format("Failed to load assembly from '{0}'", assemblyPath));
            }

            return assy.GetAllTypes().Select(x => new CciTypeDetails(x));
        }
    }
}

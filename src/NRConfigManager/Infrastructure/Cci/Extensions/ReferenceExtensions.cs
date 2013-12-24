using Microsoft.Cci;
using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci.Extensions
{
    public static class ReferenceExtensions
    {
        public static InstrumentAttribute GetInstrumentAttribute(this IReference reference)
        {
            var attributes = reference.Attributes ?? Enumerable.Empty<ICustomAttribute>();
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
    }
}

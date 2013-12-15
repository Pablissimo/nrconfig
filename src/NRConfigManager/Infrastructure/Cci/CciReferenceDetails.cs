using Microsoft.Cci;
using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci
{
    public abstract class CciReferenceDetails : IInstrumentable
    {
        private IReference _reference;

        public bool IsCompilerGenerated
        {
            get
            {
                return
                    _reference
                    .Attributes
                    .Any(x => TypeHelper.GetTypeName(x.Type) == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).FullName);
            }
        }

        public InstrumentAttribute InstrumentationContext
        {
            get
            {
                return this.GetAttributeFromType(_reference);
            }
        }

        public abstract ITypeDetails DeclaringType { get; }

        public CciReferenceDetails(IReference reference)
        {
            _reference = reference;
        }

        private InstrumentAttribute GetAttributeFromType(IReference reference)
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

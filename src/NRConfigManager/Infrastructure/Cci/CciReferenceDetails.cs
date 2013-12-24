using Microsoft.Cci;
using NRConfig;
using NRConfigManager.Infrastructure.Cci.Extensions;
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
                return _reference.GetInstrumentAttribute();
            }
        }

        public abstract ITypeDetails DeclaringType { get; }

        public CciReferenceDetails(IReference reference)
        {
            _reference = reference;
        }
    }
}

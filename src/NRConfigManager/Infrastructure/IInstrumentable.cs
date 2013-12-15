using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public interface IInstrumentable
    {
        ITypeDetails DeclaringType { get; }
        InstrumentAttribute InstrumentationContext { get; }
        bool IsCompilerGenerated { get; }
    }
}

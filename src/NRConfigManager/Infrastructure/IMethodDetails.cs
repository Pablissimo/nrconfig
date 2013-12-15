using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public interface IMethodDetails : IInstrumentable
    {
        bool ContainsGenericParameters { get; }
        ITypeDetails[] GenericArguments { get; }
        IParameterDetails[] Parameters { get; }
        string Name { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public interface IParameterDetails
    {
        int Index { get; }
        ITypeDetails Type { get; }
    }
}

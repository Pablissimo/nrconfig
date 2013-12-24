using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public interface IAssemblyDetails : IInstrumentable
    {
        string FullName { get; }
        string Name { get; }
    }
}

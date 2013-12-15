using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure
{
    public interface IPropertyDetails : IInstrumentable
    {
        bool CanGet { get; }
        bool CanSet { get; }

        IMethodDetails GetMethod { get; }
        IMethodDetails SetMethod { get; }

        string Name { get; }
    }
}

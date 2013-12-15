using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedInstrumentationDiscoverer : InstrumentationDiscovererBase
    {
        protected override IEnumerable<ITypeDetails> GetTypes(string assemblyPath)
        {
            var assy = Assembly.LoadFrom(assemblyPath);

            return
                assy
                .GetTypes()
                .Select(x => new ReflectedTypeDetails(x));
        }
    }
}

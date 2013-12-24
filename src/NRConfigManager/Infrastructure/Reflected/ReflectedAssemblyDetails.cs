using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NRConfigManager.Infrastructure.Reflected.Extensions;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedAssemblyDetails : IAssemblyDetails
    {
        Assembly _assembly;

        public string FullName
        {
            get { return _assembly.FullName; }
        }

        public string Name
        {
            get { return _assembly.GetName().Name; }
        }

        public ReflectedAssemblyDetails(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            _assembly = assembly;
        }

        public ITypeDetails DeclaringType
        {
            get { return null; }
        }

        public NRConfig.InstrumentAttribute InstrumentationContext
        {
            get { return _assembly.GetInstrumentAttribute(); }
        }

        public bool IsCompilerGenerated
        {
            get { return false; }
        }
    }
}

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
    public class CciAssemblyDetails : IAssemblyDetails
    {
        IUnit _unit;

        public string FullName
        {
            get { return _unit.Name.Value; }
        }

        public string Name
        {
            get { return _unit.UnitIdentity.Name.Value; }
        }
        
        public ITypeDetails DeclaringType
        {
            get { return null; }
        }

        public NRConfig.InstrumentAttribute InstrumentationContext
        {
            get 
            {
                return _unit.GetInstrumentAttribute();
            }
        }

        public bool IsCompilerGenerated
        {
            get { return false; }
        }

        public CciAssemblyDetails(IUnit unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException("unit");
            }

            _unit = unit;
        }
    }
}

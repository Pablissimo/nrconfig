using Microsoft.Cci;
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

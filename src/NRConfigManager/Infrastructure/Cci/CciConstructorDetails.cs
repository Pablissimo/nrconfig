using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci
{
    public class CciConstructorDetails : CciMethodDetails, IConstructorDetails
    {
        public CciConstructorDetails(IMethodDefinition constructorMethod)
            : base(constructorMethod)
        {

        }
    }
}

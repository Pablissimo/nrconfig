using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedConstructorDetails : ReflectedMethodDetails, IConstructorDetails
    {
        public ReflectedConstructorDetails(ConstructorInfo constructor)
            : base(constructor)
        {

        }
    }
}

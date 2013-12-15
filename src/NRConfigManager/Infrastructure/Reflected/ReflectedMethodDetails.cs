using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedMethodDetails : ReflectedMemberInfoDetails, IMethodDetails
    {
        MethodBase _methodBase;

        public bool ContainsGenericParameters
        {
            get { return _methodBase.ContainsGenericParameters; }
        }

        public ITypeDetails[] GenericArguments
        {
            get
            {
                return
                    _methodBase
                    .GetGenericArguments()
                    .Select(x => new ReflectedTypeDetails(x))
                    .ToArray();
            }
        }

        public IParameterDetails[] Parameters
        {
            get
            {
                return
                    _methodBase
                    .GetParameters()
                    .Select(x => new ReflectedParameterDetails(x))
                    .ToArray();
            }
        }

        public string Name
        {
            get
            {
                return _methodBase.Name;
            }
        }

        public ReflectedMethodDetails(MethodBase methodBase)
            : base(methodBase)
        {
            _methodBase = methodBase;
        }
    }
}

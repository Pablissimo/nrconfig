using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedParameterDetails : IParameterDetails
    {
        ParameterInfo _parameter;

        public int Index
        {
            get { return _parameter.Position; }
        }

        public ITypeDetails Type
        {
            get { return new ReflectedTypeDetails(_parameter.ParameterType); }
        }

        public ReflectedParameterDetails(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            _parameter = parameter;
        }
    }
}

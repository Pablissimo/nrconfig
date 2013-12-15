using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci
{
    public class CciParameterDetails : IParameterDetails
    {
        IParameterDefinition _parameter;

        public int Index
        {
            get { return _parameter.Index; }
        }

        public ITypeDetails Type
        {
            get { return new CciTypeDetails(_parameter.Type as INamedTypeDefinition); }
        }

        public CciParameterDetails(IParameterDefinition parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            _parameter = parameter;
        }
    }
}

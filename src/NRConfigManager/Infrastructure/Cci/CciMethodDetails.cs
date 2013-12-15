using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci
{
    public class CciMethodDetails : CciReferenceDetails, IMethodDetails
    {
        IMethodDefinition _method;

        public bool ContainsGenericParameters
        {
            get 
            {
                return _method.IsGeneric;
            }
        }

        public ITypeDetails[] GenericArguments
        {
            get
            {
                return 
                    _method
                    .GenericParameters
                    .Select(x => new CciTypeDetails(x))
                    .ToArray();
            }
        }

        public IParameterDetails[] Parameters
        {
            get
            {
                return
                    _method
                    .Parameters
                    .Select(x => new CciParameterDetails(x))
                    .ToArray();
            }
        }

        public string Name
        {
            get
            {
                return _method.Name.Value;
            }
        }

        public override ITypeDetails DeclaringType
        {
            get { return new CciTypeDetails(_method.ContainingTypeDefinition as INamedTypeDefinition); }
        }

        public CciMethodDetails(IMethodDefinition method)
            : base(method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            _method = method;
        }
    }
}

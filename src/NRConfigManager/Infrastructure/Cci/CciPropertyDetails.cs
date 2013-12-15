using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Cci
{
    public class CciPropertyDetails : CciReferenceDetails, IPropertyDetails
    {
        IPropertyDefinition _property;

        public bool CanGet
        {
            get { return _property.Getter != null; }
        }

        public bool CanSet
        {
            get { return _property.Setter != null; }
        }
        
        public IMethodDetails GetMethod
        {
            get 
            {
                if (_property.Getter != null)
                {
                    return new CciMethodDetails(_property.Getter.ResolvedMethod);
                }
                else
                {
                    return null;
                }
            }
        }

        public IMethodDetails SetMethod
        {
            get 
            {
                if (_property.Setter != null)
                {
                    return new CciMethodDetails(_property.Setter.ResolvedMethod);
                }
                else
                {
                    return null;
                }
            }
        }

        public string Name
        {
            get
            {
                return TypeHelper.GetTypeName(_property.ContainingType, NameFormattingOptions.OmitContainingNamespace | NameFormattingOptions.OmitContainingType);
            }
        }

        public override ITypeDetails DeclaringType
        {
            get
            {
                return new CciTypeDetails(_property.ContainingTypeDefinition as INamedTypeDefinition);
            }
        }

        public CciPropertyDetails(IPropertyDefinition property)
            : base(property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            _property = property;
        }
    }
}

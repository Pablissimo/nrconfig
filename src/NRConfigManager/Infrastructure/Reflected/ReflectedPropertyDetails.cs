using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedPropertyDetails : ReflectedMemberInfoDetails, IPropertyDetails
    {
        PropertyInfo _propertyInfo;

        public bool CanGet
        {
            get { return _propertyInfo.CanRead; }
        }

        public bool CanSet
        {
            get { return _propertyInfo.CanWrite; }
        }

        public IMethodDetails GetMethod
        {
            get 
            {
                if (_propertyInfo.CanRead)
                {
                    return new ReflectedMethodDetails(_propertyInfo.GetMethod);
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
                if (_propertyInfo.CanWrite)
                {
                    return new ReflectedMethodDetails(_propertyInfo.SetMethod);
                }
                else
                {
                    return null;
                }
            }
        }

        public string Name
        {
            get { return _propertyInfo.Name; }
        }

        public ReflectedPropertyDetails(PropertyInfo property)
            : base(property)
        {
            _propertyInfo = property;
        }
    }
}

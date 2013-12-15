using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected
{
    public class ReflectedMemberInfoDetails : IInstrumentable
    {
        MemberInfo _memberInfo;

        public NRConfig.InstrumentAttribute InstrumentationContext
        {
            get
            {
                // Since we might have assy version differences, we're stuck buggering about with reflection
                Attribute matchingAttribute = _memberInfo.GetCustomAttributes().FirstOrDefault(x => x.GetType().Name.EndsWith("InstrumentAttribute"));

                InstrumentAttribute toReturn = null;

                if (matchingAttribute != null)
                {
                    toReturn = new InstrumentAttribute();
                    Type matchingAttributeType = matchingAttribute.GetType();
                    foreach (var prop in matchingAttributeType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
                    {
                        PropertyInfo equivalentProperty = typeof(InstrumentAttribute).GetProperty(prop.Name);
                        if (equivalentProperty != null)
                        {
                            if (prop.PropertyType.IsEnum)
                            {
                                equivalentProperty.SetValue(toReturn, Convert.ToInt32(prop.GetValue(matchingAttribute)));
                            }
                            else
                            {
                                equivalentProperty.SetValue(toReturn, prop.GetValue(matchingAttribute));
                            }
                        }
                    }
                }

                return toReturn;
            }
        }

        public bool IsCompilerGenerated
        {
            get { return _memberInfo.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null; }
        }

        public ITypeDetails DeclaringType
        {
            get { return new ReflectedTypeDetails(_memberInfo.DeclaringType); }
        }

        public ReflectedMemberInfoDetails(MemberInfo memberInfo)
        {
            _memberInfo = memberInfo;
        }
    }
}

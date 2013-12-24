using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Infrastructure.Reflected.Extensions
{
    public static class AttributeProviderExtensions
    {
        public static InstrumentAttribute GetInstrumentAttribute(this ICustomAttributeProvider provider)
        {
            // Since we might have assy version differences, we're stuck buggering about with reflection
            Attribute matchingAttribute = provider.GetCustomAttributes(false).OfType<Attribute>().FirstOrDefault(x => x.GetType().Name.EndsWith("InstrumentAttribute"));

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
}

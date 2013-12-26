using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestSupport
{
    public static class EqualityHelper
    {
        public static bool AreObjectsEquivalentByPublicProperties(object obj1, object obj2, bool alsoInherited = true)
        {
            if (obj1 == null && obj2 == null)
            {
                // Assume nulls are equivalent
                return true;
            }
            else if ((obj1 == null && obj2 != null) || (obj2 == null && obj1 != null))
            {
                return false;
            }

            var obj1Type = obj1.GetType();
            var obj2Type = obj2.GetType();

            if (obj1Type != obj2Type)
            {
                return false;
            }

            var flags = BindingFlags.Public | BindingFlags.Instance;
            if (!alsoInherited)
            {
                flags |= BindingFlags.DeclaredOnly;
            }

            var props = obj1Type.GetProperties(flags);

            foreach (var prop in props)
            {
                var obj1Value = prop.GetValue(obj1);
                var obj2Value = prop.GetValue(obj2);

                if (obj1 == null && obj2 == null)
                {
                    // Nothing more to do with this property
                }
                else if ((obj1 == null && obj2 != null) || (obj2 == null && obj1 != null))
                {
                    // If one value's null and the other isn't, the two objects clearly
                    // aren't equivalent
                    return false;
                }
                else
                {
                    // If the types of the two values are 'basic' CLR types
                    // (i.e. System.String, System.Decimal, System.Int32 etc), or enums or
                    // structs then we can directly compare them
                    if (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string) || Nullable.GetUnderlyingType(prop.PropertyType) != null)
                    {
                        if (!obj1Value.Equals(obj2Value))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Complex type so we're recursing
                        bool equivalent = AreObjectsEquivalentByPublicProperties(obj1Value, obj2Value);
                        if (!equivalent)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}

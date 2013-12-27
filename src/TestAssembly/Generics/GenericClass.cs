using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.Generics
{
    public class GenericClass<T>
    {
        public void Method<U>()
        {
        }

        public void OtherMethod(T parameterOne, Action<T> parameter2)
        {
        }

        public void MixedAruments<U>(T parameterOne, U parameter2)
        {
        }
    }
}

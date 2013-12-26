using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.Generics
{
    public class GenericMethods
    {
        public void NonGenericMethod(string parameter)
        {
        }

        public void OneParameterSimpleMethod<T>(T simpleParameter)
        {
        }

        public void OneParameterComplexMethod<T>(System.Action<T> complexParameter)
        {
        }

        public void OneParameterDoubleComplexMethod<T>(System.Action<IEnumerable<T>> doubleComplexParameter)
        {
        }

        public void TwoParameterMethod<T, U>(T first, string middle, U second)
        {
        }

        public void TwoParameterMethod<T, U>(T first, U second, T third)
        {
        }

        public void TwoParameterComplexMethod<T, U>(IEnumerable<U> first, Func<T, U, T> second)
        {
        }

        public void TwoParameterDoubleComplexMethod<T, U>(IEnumerable<U> first, Func<IEnumerable<T>, U, T> second)
        {
        }
    }
}

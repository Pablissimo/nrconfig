using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.Generics
{
    public class ClosedGenericParameters
    {
        public void MethodWithTwoParameterGenericTypeInSignature(IEnumerable<string> first, Func<IList<bool>, IEnumerable<int>> second)
        {
            // Wouldn't expect this method to show up in instrumentation with parameters as we can't express the Func`2 in a way
            // New Relic'll process
        }

        public void MethodWithTwoSingleParameterGenericTypesInSignature(IEnumerable<string> first, IList<int> second)
        {
        }
    }
}

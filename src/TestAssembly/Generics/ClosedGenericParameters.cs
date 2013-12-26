using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.Generics
{
    public class ClosedGenericParameters
    {
        public void Method(IEnumerable<string> first, Func<IList<bool>, IEnumerable<int>> second)
        {
        }
    }
}

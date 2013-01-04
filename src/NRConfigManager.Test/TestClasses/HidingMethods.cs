using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.TestClasses
{
    public class HidingMethodsBase
    {
        public void MyMethod()
        {

        }
    }

    public class HidingMethodsExtended : HidingMethodsBase
    {
        public new void MyMethod()
        {

        }
    }
}

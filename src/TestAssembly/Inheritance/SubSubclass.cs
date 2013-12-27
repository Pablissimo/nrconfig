using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.Inheritance
{
    public class SubSubclass : Subclass
    {
        public override void VirtualMethod1(string parameter)
        {
            base.VirtualMethod1(parameter);
        }

        public override void VirtualMethod2(string parameter)
        {
            
        }
    }
}

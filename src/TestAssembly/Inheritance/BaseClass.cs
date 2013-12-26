using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.Inheritance
{
    public abstract class BaseClass
    {
        public void NonVirtualMethod(string parameter)
        {
        }

        public virtual void VirtualMethod(string parameter)
        {
        }

        public abstract void AbstractMethod();
    }
}

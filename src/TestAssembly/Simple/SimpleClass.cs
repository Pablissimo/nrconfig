using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAssembly.Simple
{
    public class SimpleClass
    {
        public string StringProperty { get; set; }

        public static string StaticStringProperty { get; set; }

        public bool GetterOnly { get { return false; } }
        public bool SetterOnly { set { } }

        public event EventHandler SimpleEvent;

        public SimpleClass(string parameter)
        {
        }

        private SimpleClass(int otherParameter)
        {
        }

        public static string StaticMethod()
        {
            return null;
        }

        public static void StaticParameterised(int parameter)
        {
        }
    }
}

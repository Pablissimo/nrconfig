using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.TestClasses
{
    public class HidingPropertiesBase
    {
        public bool Property { get; private set; }
    }

    public class HidingPropertiesExtended : HidingPropertiesBase
    {
        public new bool Property { get; set; }
    }
}

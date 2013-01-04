using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.TestClasses
{
    public class IndexerProperties
    {
        public virtual bool this[int index] 
        {
            get { return false; }
            set { }
        }

        public virtual bool this[string key]
        {
            get { return false; }
            set { }
        }
    }

    public class IndexerPropertiesExtended : IndexerProperties
    {
        public override bool this[int index]
        {
            get { return false; }
            set { }
        }

        public override bool this[string key]
        {
            get { return false; }
            set { }
        }
    }
}

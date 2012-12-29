using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Rendering
{
    public class Instrumentation
    {
        public List<TracerFactory> TracerFactories { get; set; }

        public Instrumentation()
        {
            this.TracerFactories = new List<TracerFactory>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Rendering
{
    public class Match
    {
        public string Metric { get; set; }
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }

        public List<ExactMethodMatcher> Matches { get; set; }

        public Match()
        {
            this.Matches = new List<ExactMethodMatcher>();
        }

        public Match(string metric, string assemblyName, string className)
            : this()
        {
            this.Metric = metric;
            this.AssemblyName = assemblyName;
            this.ClassName = className;
        }
    }
}

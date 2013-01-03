using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NRConfigManager.Rendering
{
    /// <summary>
    /// Class for New Relic-compatible XML output of the match element
    /// </summary>
    public class Match
    {
        [XmlAttribute(AttributeName="assemblyName")]
        public string AssemblyName { get; set; }
        [XmlAttribute(AttributeName = "className")]
        public string ClassName { get; set; }

        [XmlElement(ElementName="exactMethodMatcher")]
        public List<ExactMethodMatcher> Matches { get; set; }

        public Match()
        {
            this.Matches = new List<ExactMethodMatcher>();
        }

        public Match(string assemblyName, string className)
            : this()
        {
            this.AssemblyName = assemblyName;
            this.ClassName = className;
        }
    }
}

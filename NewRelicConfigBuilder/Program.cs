using NewRelicConfigManager.Infrastructure;
using NewRelicConfigManager.Rendering;
using NewRelicConfigManager.Test.TestClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewRelicConfigBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderedExplicit = new Renderer().Render(InstrumentationDiscoverer.GetInstrumentationSet(typeof(ExplicitMarkup)));
            var renderedImplicit = new Renderer().Render(InstrumentationDiscoverer.GetInstrumentationSet(typeof(ClassLevelImplicitMarkup)));

            var renderedOverrides = new Renderer().Render(InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup)));

            
            // Render to XML
            XmlSerializer ser = new XmlSerializer(typeof(Extension));

            Console.WriteLine();
            Console.WriteLine("Explicit:");
            ser.Serialize(Console.Out, renderedExplicit);

            Console.WriteLine("\n\nImplicit:");
            ser.Serialize(Console.Out, renderedImplicit);

            Console.WriteLine("\n\nOverrides:");
            ser.Serialize(Console.Out, renderedOverrides);

            Console.Read();
        }
    }
}

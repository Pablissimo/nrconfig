using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfigManager.Infrastructure.Reflected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Rendering.DiscovererSpecific
{
    [TestClass]
    public class ReflectedRendererCorrectnessTests : RendererCorrectnessTestBase
    {
        protected override NRConfigManager.Infrastructure.InstrumentationDiscovererBase GetDiscoverer()
        {
            return new ReflectedInstrumentationDiscoverer();
        }
    }
}

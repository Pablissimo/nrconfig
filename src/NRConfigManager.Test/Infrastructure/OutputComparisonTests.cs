using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSupport;

namespace NRConfigManager.Test.Infrastructure
{
    [TestClass]
    public class OutputComparisonTests
    {
        [TestMethod]
        public void CciAndReflectedStrategies_ProduceSameOutput_WithNoExplicitInitialContext()
        {
            var cci = new NRConfigManager.Infrastructure.Cci.CciInstrumentationDiscoverer();
            var reflected = new NRConfigManager.Infrastructure.Reflected.ReflectedInstrumentationDiscoverer();

            var cciSet = cci.GetInstrumentationSet("TestAssembly.dll", null, x => true);
            var reflectedSet = reflected.GetInstrumentationSet("TestAssembly.dll", null, x => true);

            // Render both
            var cciSetRendered = NRConfigManager.Rendering.Renderer.Render(cciSet);
            var reflectedSetRendered = NRConfigManager.Rendering.Renderer.Render(reflectedSet);

            Assert.IsTrue(EqualityHelper.AreObjectsEquivalentByPublicProperties(cciSetRendered, reflectedSetRendered, true));
        }

        [TestMethod]
        public void CciAndReflectedStrategies_ProduceSameOutput_WithExplicitAllPublicNonPublicContext()
        {
            var cci = new NRConfigManager.Infrastructure.Cci.CciInstrumentationDiscoverer();
            var reflected = new NRConfigManager.Infrastructure.Reflected.ReflectedInstrumentationDiscoverer();

            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.All };

            var cciSet = cci.GetInstrumentationSet("TestAssembly.dll", context, x => true);
            var reflectedSet = reflected.GetInstrumentationSet("TestAssembly.dll", context, x => true);

            // Render both
            var cciSetRendered = NRConfigManager.Rendering.Renderer.Render(cciSet);
            var reflectedSetRendered = NRConfigManager.Rendering.Renderer.Render(reflectedSet);

            Assert.IsTrue(EqualityHelper.AreObjectsEquivalentByPublicProperties(cciSetRendered, reflectedSetRendered, true));
        }
    }
}

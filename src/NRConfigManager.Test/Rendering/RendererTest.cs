using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfigManager.Infrastructure;
using NRConfigManager.Rendering;
using NRConfigManager.Test.TestClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Rendering
{
    [TestClass]
    public class RendererTest
    {
        [TestMethod]
        public void GetMatcherFromTarget_HandlesGenericMethodParameters_WhenOnlyOne()
        {
            var target =
                InstrumentationDiscoverer
                .GetInstrumentationSet(typeof(GenericsTest))
                .Where(x => x.Target.Name == "OneParameterMethod")
                .FirstOrDefault();

            Assert.IsNotNull(target);

            var matcher = Renderer.GetMatcherFromTarget(target);

            Assert.AreEqual("OneParameterMethod", matcher.MethodName);
            Assert.AreEqual("<MVAR 0>", matcher.ParameterTypes);
        }

        [TestMethod]
        public void GetMatcherFromTarget_HandlesGenericParameters_WhenMultipleAndAllUsed()
        {
            var target =
                InstrumentationDiscoverer
                .GetInstrumentationSet(typeof(GenericsTest))
                .Where(x => x.Target.Name == "TwoParameterMethod" && x.Target.GetParameters().Length == 2)
                .FirstOrDefault();

            Assert.IsNotNull(target);

            var matcher = Renderer.GetMatcherFromTarget(target);

            Assert.AreEqual("TwoParameterMethod", matcher.MethodName);
            Assert.AreEqual("<MVAR 0>,<MVAR 1>", matcher.ParameterTypes);
        }

        [TestMethod]
        public void GetMatcherFromTarget_HandlesGenericParameters_WhenMultipleAndSomeUnused()
        {
            var target =
                InstrumentationDiscoverer
                .GetInstrumentationSet(typeof(GenericsTest))
                .Where(x => x.Target.Name == "ThreeParameterMethod")
                .FirstOrDefault();

            Assert.IsNotNull(target);

            var matcher = Renderer.GetMatcherFromTarget(target);

            Assert.AreEqual("ThreeParameterMethod", matcher.MethodName);
            Assert.AreEqual("<MVAR 0>,<MVAR 2>", matcher.ParameterTypes); // Note - no MVAR1
        }

        [TestMethod]
        public void GetMatcherFromTarget_HandlesGenericParameters_WhenMixOfGenericAndNonGeneric()
        {
            var target =
                InstrumentationDiscoverer
                .GetInstrumentationSet(typeof(GenericsTest))
                .Where(x => x.Target.Name == "MixedMethod")
                .FirstOrDefault();

            Assert.IsNotNull(target);

            var matcher = Renderer.GetMatcherFromTarget(target);

            Assert.AreEqual("MixedMethod", matcher.MethodName);
            Assert.AreEqual("<MVAR 0>,System.String,<MVAR 1>", matcher.ParameterTypes);
        }
    }
}

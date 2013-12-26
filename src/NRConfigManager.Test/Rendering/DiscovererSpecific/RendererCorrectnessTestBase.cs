using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfig;
using NRConfigManager.Infrastructure;
using NRConfigManager.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Rendering.DiscovererSpecific
{
    public abstract class RendererCorrectnessTestBase
    {
        InstrumentationDiscovererBase _discoverer;

        [TestInitialize]
        public void Initialise()
        {
            _discoverer = this.GetDiscoverer();
        }

        [TestMethod]
        public void Renderer_UsesBacktickAngleBracketNotation_ForSingleArgumentClosedGenericParameters()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.ClosedGenericParameters).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "MethodWithTwoSingleParameterGenericTypesInSignature");

            Assert.AreEqual("System.Collections.Generic.IEnumerable`1<System.String>,System.Collections.Generic.IList`1<System.Int32>", method.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_OmitsParameterList_ForTwoArgumentClosedGenericParameters()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.ClosedGenericParameters).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "MethodWithTwoParameterGenericTypeInSignature");

            // Expecting empty string because New Relic can't handle two-parameter generics
            Assert.AreEqual(string.Empty, method.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_HandlesSimpleSingleGenericParameterMethods()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.GenericMethods).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "OneParameterSimpleMethod");

            Assert.AreEqual("<MVAR 0>", method.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_HandlesComplexSingleGenericParameterMethods()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.GenericMethods).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "OneParameterComplexMethod");

            Assert.AreEqual("System.Action`1<MVAR 0>", method.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_HandlesDoublyComplexSingleGenericParameterMethods()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.GenericMethods).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "OneParameterDoubleComplexMethod");

            Assert.AreEqual("System.Action`1<System.Collections.Generic.IEnumerable`1<MVAR 0>>", method.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_HandlesSimpleTwoGenericParameterMethods()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.GenericMethods).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "TwoParameterMethod");

            Assert.AreEqual("<MVAR 0>,<MVAR 1>,<MVAR 0>", method.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_HandlesComplexTwoGenericParameterMethods()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.GenericMethods).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "TwoParameterComplexMethod");

            Assert.AreEqual("System.Collections.Generic.IEnumerable`1<MVAR 1>,System.Collections.Generic.IList`1<MVAR 0>", method.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_HandlesDoublyComplexTwoGenericParameterMethods()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Generics.GenericMethods).Name);
            var method = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == "TwoParameterDoubleComplexMethod");

            Assert.AreEqual("System.Collections.Generic.IEnumerable`1<MVAR 1>,System.Func`1<System.Collections.Generic.IEnumerable`1<MVAR 0>>", method.ParameterTypes);
        }

        private Extension RenderWhere(Func<ITypeDetails, bool> whereClause)
        {
            var set = _discoverer.GetInstrumentationSet("TestAssembly.dll", new InstrumentAttribute { Scopes = InstrumentationScopes.All }, x => whereClause(x));
            return Renderer.Render(set);
        }

        protected abstract InstrumentationDiscovererBase GetDiscoverer();
    }
}

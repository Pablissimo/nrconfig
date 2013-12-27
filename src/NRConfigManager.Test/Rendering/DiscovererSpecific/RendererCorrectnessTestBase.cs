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

        [TestMethod]
        public void Renderer_IncludesExplicitConstructors()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.SimpleClass).Name);
            var ctors = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == ".ctor");

            Assert.IsTrue(ctors.Any(x => x.ParameterTypes == "System.Int32"));
            Assert.IsTrue(ctors.Any(x => x.ParameterTypes == "System.String"));
            Assert.AreEqual(2, ctors.Count());
        }

        [TestMethod]
        public void Renderer_ExcludesImplicitStaticConstructors()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.SimpleClass).Name);
            var ctorCount = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Count(x => x.MethodName == ".cctor");

            Assert.AreEqual(0, ctorCount);
        }

        [TestMethod]
        public void Renderer_IncludesImplicitConstructors()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.ImplicitConstructorExplicitStaticConstructor).Name);
            var ctor = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == ".ctor");

            Assert.AreEqual("void", ctor.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_IncludesExplicitStaticConstructors()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.ImplicitConstructorExplicitStaticConstructor).Name);
            var ctor = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Single(x => x.MethodName == ".cctor");

            Assert.AreEqual("void", ctor.ParameterTypes);
        }

        [TestMethod]
        public void Renderer_IncludesAbstractMethods_InClassTheyAreImplemented()
        {
            var resultBase = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.BaseClass).Name);
            var methodBase = resultBase.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "AbstractMethod");

            var resultSubtype = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.Subclass).Name);
            var methodSubtype = resultSubtype.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "AbstractMethod");

            Assert.AreEqual(0, methodBase.Count());

            Assert.AreEqual(1, methodSubtype.Count());
            Assert.AreEqual("void", methodSubtype.First().ParameterTypes);
        }

        [TestMethod]
        public void Renderer_IncludesOverriddenVirtualMethods_InAllClassesTheyAreImplemented()
        {
            // VirtualMethod1 appears in all three classes in the BaseClass -> Subclass -> SubSubclass hierarchy
            var resultBase = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.BaseClass).Name);
            var methodBase = resultBase.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "VirtualMethod1");

            var resultSubtype = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.Subclass).Name);
            var methodSubtype = resultSubtype.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "VirtualMethod1");

            var resultSubSubtype = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.SubSubclass).Name);
            var methodSubSubtype = resultSubSubtype.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "VirtualMethod1");

            Assert.AreEqual(1, methodBase.Count());
            Assert.AreEqual("System.String", methodBase.First().ParameterTypes);

            Assert.AreEqual(1, methodSubtype.Count());
            Assert.AreEqual("System.String", methodSubtype.First().ParameterTypes);

            Assert.AreEqual(1, methodSubSubtype.Count());
            Assert.AreEqual("System.String", methodSubSubtype.First().ParameterTypes);
        }

        [TestMethod]
        public void Renderer_ExcludesOverriddenVirtualMethods_FromClassesWhereTheyAreNotOverridden()
        {
            // VirtualMethod2 appears in two of the three classes in the BaseClass -> Subclass -> SubSubclass hierarchy - the first and last
            var resultBase = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.BaseClass).Name);
            var methodBase = resultBase.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "VirtualMethod2");

            var resultSubtype = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.Subclass).Name);
            var methodSubtype = resultSubtype.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "VirtualMethod2");

            var resultSubSubtype = RenderWhere(x => x.Name == typeof(TestAssembly.Inheritance.SubSubclass).Name);
            var methodSubSubtype = resultSubSubtype.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName == "VirtualMethod2");

            Assert.AreEqual(1, methodBase.Count());
            Assert.AreEqual("System.String", methodBase.First().ParameterTypes);

            Assert.AreEqual(0, methodSubtype.Count());

            Assert.AreEqual(1, methodSubSubtype.Count());
            Assert.AreEqual("System.String", methodSubSubtype.First().ParameterTypes);
        }

        [TestMethod]
        public void Renderer_IncludesGettersAndSetters_ForPropertiesThatAreReadWrite()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.SimpleClass).Name);
            var propAccessors = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName.EndsWith("_StringProperty"));

            Assert.AreEqual(2, propAccessors.Count());
            Assert.IsTrue(propAccessors.Any(x => x.MethodName == "get_StringProperty"));
            Assert.IsTrue(propAccessors.Any(x => x.MethodName == "set_StringProperty"));
        }

        [TestMethod]
        public void Renderer_IncludesGettersAndSetters_ForStaticPropertiesThatAreReadWrite()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.SimpleClass).Name);
            var propAccessors = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName.EndsWith("_StaticStringProperty"));

            Assert.AreEqual(2, propAccessors.Count());
            Assert.IsTrue(propAccessors.Any(x => x.MethodName == "get_StaticStringProperty"));
            Assert.IsTrue(propAccessors.Any(x => x.MethodName == "set_StaticStringProperty"));
        }

        [TestMethod]
        public void Renderer_IncludesGetterButNoSetter_ForPropertiesThatAreReadOnly()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.SimpleClass).Name);
            var propAccessors = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName.EndsWith("_GetterOnly"));

            Assert.AreEqual(1, propAccessors.Count());
            Assert.IsTrue(propAccessors.Any(x => x.MethodName == "get_GetterOnly"));
            Assert.IsFalse(propAccessors.Any(x => x.MethodName == "set_GetterOnly"));
        }

        [TestMethod]
        public void Renderer_IncludesGetterButNoSetter_ForPropertiesThatAreWriteOnly()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.SimpleClass).Name);
            var propAccessors = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName.EndsWith("_SetterOnly"));

            Assert.AreEqual(1, propAccessors.Count());
            Assert.IsFalse(propAccessors.Any(x => x.MethodName == "get_SetterOnly"));
            Assert.IsTrue(propAccessors.Any(x => x.MethodName == "set_SetterOnly"));
        }

        [TestMethod]
        public void Renderer_ExcludesEventAddAndRemoveHandlers()
        {
            var result = RenderWhere(x => x.Name == typeof(TestAssembly.Simple.SimpleClass).Name);
            var eventAccessors  = result.Instrumentation.TracerFactories.SelectMany(x => x.MatchDefinitions).SelectMany(x => x.Matches).Where(x => x.MethodName.EndsWith("_SimpleEvent"));

            Assert.IsFalse(eventAccessors.Any(x => x.MethodName == "add_SimpleEvent"));
            Assert.IsFalse(eventAccessors.Any(x => x.MethodName == "remove_SimpleEvent"));
        }

        private Extension RenderWhere(Func<ITypeDetails, bool> whereClause)
        {
            var set = _discoverer.GetInstrumentationSet("TestAssembly.dll", new InstrumentAttribute { Scopes = InstrumentationScopes.All }, x => whereClause(x));
            return Renderer.Render(set);
        }

        protected abstract InstrumentationDiscovererBase GetDiscoverer();
    }
}

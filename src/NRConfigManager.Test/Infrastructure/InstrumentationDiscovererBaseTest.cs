using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfig;
using NRConfigManager.Infrastructure;
using NRConfigManager.Test.Infrastructure.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestSupport;

namespace NRConfigManager.Test.Infrastructure
{
    [TestClass]
    public class InstrumentationDiscovererBaseTest
    {
        IEnumerable<DummyTypeDetails> _dummyTypes;
        TestInstrumentationDiscoverer _testDiscoverer;

        DummyTypeDetails _structType;
        DummyTypeDetails _classType;
        DummyTypeDetails _nestedType;
        DummyTypeDetails _generatedType;

        [TestInitialize]
        public void Initialise()
        {
            DummyAssembly.Reset();

            _structType = new DummyTypeDetails("Test.Struct") { IsClass = false };
            _classType = new DummyTypeDetails("Test.Class");
            _nestedType = new DummyTypeDetails("Test.Class.Nested") { IsNested = true, DeclaringType = _classType };

            _classType.SetNestedTypes(new[] 
            {
                _nestedType
            });

            _generatedType = new DummyTypeDetails("Test.Class.Generated") { IsCompilerGenerated = true };

            _classType.SetMethods(new DummyMethodDetails[] 
            {
                new DummyMethodDetails(_classType, "GeneratedMethod") { IsCompilerGenerated = true },
                new DummyMethodDetails(_classType, "VoidMethod")
            });

            _dummyTypes = new DummyTypeDetails[] 
            {
                _structType,
                _classType,
                _nestedType,
                _generatedType
            };

            _testDiscoverer = new TestInstrumentationDiscoverer(_dummyTypes);
        }

        [TestMethod]
        public void GetInstrumentationSet_SkipsNestedTypes()
        {
            _classType.SetNestedTypes(null);

            _testDiscoverer.GetInstrumentationSet("dummy path", null, x => true);

            Assert.IsFalse(_testDiscoverer.ScannedTypes.ContainsKey(_nestedType));
        }

        [TestMethod]
        public void GetInstrumentationSet_SkipsStructs()
        {
            _testDiscoverer.GetInstrumentationSet("dummy path", null, x => true);

            Assert.IsFalse(_testDiscoverer.ScannedTypes.ContainsKey(_structType));
        }

        [TestMethod]
        public void GetInstrumentationSet_UsesSuppliedPredicateToFilterTypes()
        {
            // Disallow all types
            _testDiscoverer.GetInstrumentationSet("dummy path", null, x => false);

            Assert.IsFalse(_testDiscoverer.ScannedTypes.Any());
        }

        [TestMethod]
        public void GetInstrumentationSet_UsesSuppliedInitialContext()
        {
            var context = new InstrumentAttribute { Metric = Metric.Scoped, MetricName = "Test metric", Scopes = InstrumentationScopes.All };

            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => true);

            EqualityHelper.AreObjectsEquivalentByPublicProperties(_testDiscoverer.FirstContext, context, false);
        }

        [TestMethod]
        public void GetInstrumentationSet_UsesAssemblyLevelContext_IfNoneExplicitlySupplied()
        {
            var assyContext = new InstrumentAttribute { Metric = Metric.Scoped, MetricName = "Test metric", Scopes = InstrumentationScopes.All };
            DummyAssembly.Instance.InstrumentationContext = assyContext;

            _testDiscoverer.GetInstrumentationSet("dummy path", null, x => true);

            EqualityHelper.AreObjectsEquivalentByPublicProperties(_testDiscoverer.FirstContext, assyContext, false);
        }

        [TestMethod]
        public void GetInstrumentationSet_MergesInitialAndAssemblyLevelContext_IfBothSupplied()
        {
            var initialContext = new InstrumentAttribute { Metric = Metric.Unscoped, MetricName = "Initial metric", Scopes = InstrumentationScopes.All, IncludeCompilerGeneratedCode = true };
            var assyContext = new InstrumentAttribute { Metric = Metric.Scoped, MetricName = "Assy metric", Scopes = InstrumentationScopes.All };
            var expectedContext = new InstrumentAttribute { Metric = NRConfig.Metric.Scoped, MetricName = "Assy metric", Scopes = InstrumentationScopes.All, IncludeCompilerGeneratedCode = true };

            DummyAssembly.Instance.InstrumentationContext = assyContext;

            _testDiscoverer.GetInstrumentationSet("dummy path", initialContext, x => true);

            EqualityHelper.AreObjectsEquivalentByPublicProperties(_testDiscoverer.FirstContext, expectedContext, false);
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsPublicMethods_WhenPublicMethodsSpecifiedInContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.PublicMethods };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsTrue(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.Public));
            Assert.IsFalse(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsNonPublicMethods_WhenNonPublicMethodsSpecifiedInContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.NonPublicMethods };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsFalse(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.Public));
            Assert.IsTrue(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsNoMethods_WhenNotRequiredByContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.None };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsFalse(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.Public));
            Assert.IsFalse(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsStaticAndNonStaticAndDeclaredOnlyMethods()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.None };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsTrue(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.Static));
            Assert.IsTrue(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.Instance));
            Assert.IsTrue(_classType.LastGetMethodsFlags.HasFlag(BindingFlags.DeclaredOnly));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsPublicProperties_WhenPublicPropertiesSpecifiedInContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.PublicProperties };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsTrue(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.Public));
            Assert.IsFalse(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsNonPublicProperties_WhenNonPublicPropertiesSpecifiedInContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.NonPublicProperties };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsFalse(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.Public));
            Assert.IsTrue(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsNoProperties_WhenNotRequiredByContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.None };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsFalse(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.Public));
            Assert.IsFalse(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsStaticAndNonStaticAndDeclaredOnlyProperties()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.None };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsTrue(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.Static));
            Assert.IsTrue(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.Instance));
            Assert.IsTrue(_classType.LastGetPropertiesFlags.HasFlag(BindingFlags.DeclaredOnly));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsPublicConstructors_WhenPublicConstructorsSpecifiedInContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.PublicConstructors };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsTrue(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.Public));
            Assert.IsFalse(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsNonPublicConstructors_WhenNonPublicConstructorsSpecifiedInContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.NonPublicConstructors };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsFalse(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.Public));
            Assert.IsTrue(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsNoConstructors_WhenNotRequiredByContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.None };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsFalse(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.Public));
            Assert.IsFalse(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.NonPublic));
        }

        [TestMethod]
        public void GetInstrumentationSet_RequestsStaticAndNonStaticAndDeclaredOnlyConstructors()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.None };
            _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsTrue(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.Static));
            Assert.IsTrue(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.Instance));
            Assert.IsTrue(_classType.LastGetConstructorsFlags.HasFlag(BindingFlags.DeclaredOnly));
        }

        [TestMethod]
        public void GetInstrumentationSet_ExcludesGeneratedMethods()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.Methods };
            var result = _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsFalse(result.Any(x => x.Target == _classType.GetMethods(BindingFlags.Public).First(method => method.IsCompilerGenerated)));
        }

        [TestMethod]
        public void GetInstrumentationSet_IncludesGeneratedMethods_IfSpecifiedByInitialContext()
        {
            var context = new InstrumentAttribute { Scopes = InstrumentationScopes.Methods, IncludeCompilerGeneratedCode = true };
            var result = _testDiscoverer.GetInstrumentationSet("dummy path", context, x => x == _classType);

            Assert.IsTrue(result.Any(x => x.Target == _classType.GetMethods(BindingFlags.Public).First(method => method.IsCompilerGenerated)));
        }

        [TestMethod]
        public void GetInstrumentationSet_IncludesGeneratedMethods_IfSpecifiedByTypeContext()
        {
            var initialContext = new InstrumentAttribute { Scopes = InstrumentationScopes.Methods };
            var methodContext = new InstrumentAttribute { IncludeCompilerGeneratedCode = true };

            // Apply a method-level context to the generated method
            _classType.InstrumentationContext = methodContext;

            var result = _testDiscoverer.GetInstrumentationSet("dummy path", initialContext, x => x == _classType);

            Assert.IsTrue(result.Any(x => x.Target == _classType.GetMethods(BindingFlags.Public).First(method => method.IsCompilerGenerated)));
        }

        [TestMethod]
        public void GetInstrumentationSet_IncludesGeneratedMethods_IfSpecifiedByMethodContext()
        {
            var initialContext = new InstrumentAttribute { Scopes = InstrumentationScopes.Methods };
            var methodContext = new InstrumentAttribute { IncludeCompilerGeneratedCode = true };

            // Apply a method-level context to the generated method
            ((DummyMethodDetails) _classType.GetMethods(BindingFlags.Public).First(x => x.IsCompilerGenerated)).InstrumentationContext = methodContext;

            var result = _testDiscoverer.GetInstrumentationSet("dummy path", initialContext, x => x == _classType);

            Assert.IsTrue(result.Any(x => x.Target == _classType.GetMethods(BindingFlags.Public).First(method => method.IsCompilerGenerated)));
        }

        [TestMethod]
        public void GetInstrumentationSet_UsesOuterTypeContext_WhenNestedTypeHasNoExplicitContext()
        {
            var typeContext = new InstrumentAttribute { Scopes = InstrumentationScopes.All };
            _classType.InstrumentationContext = typeContext;

            _testDiscoverer.GetInstrumentationSet("dummy path", new InstrumentAttribute(), x => true);

            Assert.IsTrue(EqualityHelper.AreObjectsEquivalentByPublicProperties(typeContext, _testDiscoverer.ScannedTypes[_nestedType], false));
        }

        internal class TestInstrumentationDiscoverer : InstrumentationDiscovererBase
        {
            IEnumerable<ITypeDetails> _types;

            public InstrumentAttribute FirstContext { get; private set; }
            public Dictionary<ITypeDetails, InstrumentAttribute> ScannedTypes { get; private set; }

            public TestInstrumentationDiscoverer(IEnumerable<ITypeDetails> dummyTypes)
            {
                _types = dummyTypes;

                this.ScannedTypes = new Dictionary<ITypeDetails, InstrumentAttribute>();
            }

            protected override IEnumerable<ITypeDetails> GetTypes(string assemblyPath)
            {
                return _types;
            }

            protected override IEnumerable<InstrumentationTarget> GetInstrumentationSet(ITypeDetails t, InstrumentAttribute context)
            {
                if (this.FirstContext == null)
                {
                    this.FirstContext = context;
                }

                this.ScannedTypes[t] = context;

                return base.GetInstrumentationSet(t, context);
            }
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfig;
using NRConfigManager.Infrastructure;
using NRConfigManager.Test.Infrastructure.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestSupport;

namespace NRConfigManager.Test.Infrastructure
{
    [TestClass]
    public class InstrumentationDiscovererBaseTest
    {
        IEnumerable<ITypeDetails> _dummyTypes;
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
            _nestedType = new DummyTypeDetails("Test.Class.Nested") { IsNested = true };
            _generatedType = new DummyTypeDetails("Test.Class.Generated") { IsCompilerGenerated = true };

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
            _testDiscoverer.GetInstrumentationSet("dummy path", null, x => true);

            Assert.IsFalse(_testDiscoverer.ScannedTypes.ContainsKey(_nestedType));
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

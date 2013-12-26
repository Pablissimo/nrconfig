using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestSupport;

namespace NRConfig.Test
{
    [TestClass]
    public class InstrumentAttributeTest
    {
        [TestMethod]
        public void Constructor_InitialisesProperties_MetricNameOnly()
        {
            var attr = new InstrumentAttribute("Test Metric Name");

            Assert.AreEqual("Test Metric Name", attr.MetricName);
            Assert.IsFalse(attr.IncludeCompilerGeneratedCode);
            Assert.IsFalse(attr.IncludeCompilerGeneratedCodeSet);
        }

        [TestMethod]
        public void Constructor_InitialisesProperties_MetricNameAndMetric()
        {
            var attr = new InstrumentAttribute("Test Metric Name", Metric.Scoped);

            Assert.AreEqual(Metric.Scoped, attr.Metric);
            Assert.AreEqual("Test Metric Name", attr.MetricName);
            Assert.IsFalse(attr.IncludeCompilerGeneratedCodeSet);
            Assert.IsFalse(attr.IncludeCompilerGeneratedCode);
        }

        [TestMethod]
        public void PropertiesTest()
        {
            var attr = new InstrumentAttribute() { Metric = Metric.Scoped, MetricName = "Test Metric Name", IncludeCompilerGeneratedCode = false };

            Assert.AreEqual(Metric.Scoped, attr.Metric);
            Assert.AreEqual("Test Metric Name", attr.MetricName);
            Assert.IsTrue(attr.IncludeCompilerGeneratedCodeSet);
            Assert.IsFalse(attr.IncludeCompilerGeneratedCode);

            attr.IncludeCompilerGeneratedCode = true;
            Assert.IsTrue(attr.IncludeCompilerGeneratedCodeSet);
            Assert.IsTrue(attr.IncludeCompilerGeneratedCode);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_ReturnsNull_WhenNullAttributesSupplied()
        {
            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(null);
            Assert.IsNull(effective);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_ReturnsNull_WhenParamsArrayContainsOnlyNullEntries()
        {
            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(null, null, null);
            Assert.IsNull(effective);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_ReturnsContextEquivalentToSupplied_IfOnlyOneContextSupplied()
        {
            var attr = new InstrumentAttribute { IncludeCompilerGeneratedCode = true, Metric = Metric.Scoped, MetricName = "Metric name", Scopes = InstrumentationScopes.All };
            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(attr);

            Assert.IsTrue(EqualityHelper.AreObjectsEquivalentByPublicProperties(attr, effective, false));
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_ReturnsContextEquivalentToSupplied_IfOnlyOneNonNullContextSuppliedWithNullsBefore()
        {
            var attr = new InstrumentAttribute { IncludeCompilerGeneratedCode = true, Metric = Metric.Scoped, MetricName = "Metric name", Scopes = InstrumentationScopes.All };
            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(null, null, attr);

            Assert.IsTrue(EqualityHelper.AreObjectsEquivalentByPublicProperties(attr, effective, false));
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_ReturnsContextEquivalentToSupplied_IfOnlyOneNonNullContextSuppliedWithNullsAfter()
        {
            var attr = new InstrumentAttribute { IncludeCompilerGeneratedCode = true, Metric = Metric.Scoped, MetricName = "Metric name", Scopes = InstrumentationScopes.All };
            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(attr, null, null);

            Assert.IsTrue(EqualityHelper.AreObjectsEquivalentByPublicProperties(attr, effective, false));
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_PrioritisesEarlierContextValues_ForMetricNameProperty()
        {
            var first = new InstrumentAttribute { MetricName = "Metric name 1" };
            var second = new InstrumentAttribute { MetricName = "Metric name 2" };

            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(first, second);

            Assert.AreEqual("Metric name 1", effective.MetricName);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_PrioritisesFirstNonNullContextValue_ForMetricNameProperty()
        {
            var first = new InstrumentAttribute { MetricName = null };
            var second = new InstrumentAttribute { MetricName = "Metric name 2" };
            var third = new InstrumentAttribute { MetricName = "Metric name 3" };

            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(first, second, third);

            Assert.AreEqual("Metric name 2", effective.MetricName);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_PrioritisesEarlierContextValues_ForMetricProperty()
        {
            var first = new InstrumentAttribute { Metric = Metric.Scoped };
            var second = new InstrumentAttribute { Metric = Metric.Both };

            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(first, second);

            Assert.AreEqual(Metric.Scoped, effective.Metric);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_PrioritisesFirstSpecifiedContextValues_ForMetricProperty()
        {
            var first = new InstrumentAttribute { Metric = Metric.Unspecified };
            var second = new InstrumentAttribute { Metric = Metric.Both };
            var third = new InstrumentAttribute { Metric = Metric.Unscoped };

            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(first, second, third);

            Assert.AreEqual(Metric.Both, effective.Metric);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_PrioritisesEarlierContextValues_ForScopesProperty()
        {
            var first = new InstrumentAttribute { Scopes = InstrumentationScopes.Methods };
            var second = new InstrumentAttribute { Scopes = InstrumentationScopes.Constructors };

            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(first, second);

            Assert.AreEqual(InstrumentationScopes.Methods, effective.Scopes);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_PrioritisesEarlierContextValues_ForIncludeCompilerGeneratedCodeProperty()
        {
            var first = new InstrumentAttribute { IncludeCompilerGeneratedCode = true };
            var second = new InstrumentAttribute { IncludeCompilerGeneratedCode = false };

            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(first, second);

            Assert.IsTrue(effective.IncludeCompilerGeneratedCode);
        }

        [TestMethod]
        public void GetEffectiveInstrumentationContext_PrioritisesEarliestSetContextValues_ForIncludeCompilerGeneratedCodeProperty()
        {
            var first = new InstrumentAttribute { };
            var second = new InstrumentAttribute { IncludeCompilerGeneratedCode = true };
            var third = new InstrumentAttribute { IncludeCompilerGeneratedCode = false };

            var effective = InstrumentAttribute.GetEffectiveInstrumentationContext(first, second, third);

            Assert.IsTrue(effective.IncludeCompilerGeneratedCode);
        }
    }
}

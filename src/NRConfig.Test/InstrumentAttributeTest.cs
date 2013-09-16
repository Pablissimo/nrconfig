using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}

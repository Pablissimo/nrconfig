using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NewRelicConfiguration.Test
{
    [TestClass]
    public class InstrumentAttributeTest
    {
        [TestMethod]
        public void Constructor_InitialisesProperties_MetricNameOnly()
        {
            var attr = new InstrumentAttribute("Test Metric Name");

            Assert.AreEqual("Test Metric Name", attr.MetricName);
        }

        [TestMethod]
        public void Constructor_InitialisesProperties_MetricNameAndMetric()
        {
            var attr = new InstrumentAttribute("Test Metric Name", "Test Metric");

            Assert.AreEqual("Test Metric", attr.Metric);
            Assert.AreEqual("Test Metric Name", attr.MetricName);
        }

        [TestMethod]
        public void PropertiesTest()
        {
            var attr = new InstrumentAttribute() { Metric = "Test Metric", MetricName = "Test Metric Name" };

            Assert.AreEqual("Test Metric", attr.Metric);
            Assert.AreEqual("Test Metric Name", attr.MetricName);
        }
    }
}

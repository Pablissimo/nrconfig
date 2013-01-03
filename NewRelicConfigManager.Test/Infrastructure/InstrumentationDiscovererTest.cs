using Microsoft.VisualStudio.TestTools.UnitTesting;
using NewRelicConfigManager.Configuration;
using NewRelicConfigManager.Infrastructure;
using NewRelicConfigManager.Test.TestClasses;
using NewRelicConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigManager.Test.Infrastructure
{
    [TestClass]
    public class InstrumentationDiscovererTest
    {
        [TestMethod]
        public void InstrumentAttribute_CanOverrideMetricName()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethod && x.Method.Name == "TestMethodDifferentMetricName").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Method-level override name", ofInterest.MetricName);
            Assert.AreEqual(Metric.Scoped, ofInterest.Metric);
        }

        [TestMethod]
        public void InstrumentAttribute_CanOverrideMetricButInheritMetricName()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethod && x.Method.Name == "TestMethodDifferentMetricInheritedMetricName").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Metric name", ofInterest.MetricName);
            Assert.AreEqual(Metric.Both, ofInterest.Metric);
        }

        [TestMethod]
        public void InstrumentAttribute_CanOverrideMetricAndMetricName()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethod && x.Method.Name == "TestMethodDifferentMetricAndMetricName").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Method-level override name", ofInterest.MetricName);
            Assert.AreEqual(Metric.None, ofInterest.Metric);
        }

        [TestMethod]
        public void InstrumentAttribute_CanOverrideForNestedTypes()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethod && x.Method.Name == "OverrideMetricNameInNestedClassByDefault").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Override class-level metric name", ofInterest.MetricName);
            Assert.AreEqual(Metric.Scoped, ofInterest.Metric);
        }

        [TestMethod]
        public void MarkupTest_AllMethodsInClassesMarkedInstrumentReturned()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(ClassLevelImplicitMarkup));

            var staticCtors = result.Where(x => x.IsConstructor && x.Method.IsStatic);
            var ctors = result.Where(x => x.IsConstructor && !x.Method.IsStatic);

            var methods = result.Where(x => x.IsMethod);

            Assert.IsTrue(staticCtors.Any());
            Assert.AreEqual(4, ctors.Count());
            Assert.AreEqual(1, ctors.Count(x => x.Method.DeclaringType == typeof(ClassLevelImplicitMarkup.Nested)));

            // Single string param constructor
            Assert.IsTrue
                (
                    ctors.Any
                    (
                        x => x.Method.GetParameters() != null
                        && x.Method.GetParameters().Count() == 1
                        && x.Method.GetParameters().Any
                        (
                            y => y.ParameterType == typeof(string)
                        )
                    )
                );

            // Void constructor
            Assert.IsTrue(ctors.Any(x => x.Method.GetParameters() == null || x.Method.GetParameters().Count() == 0));

            var autoPropInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedAutoProperty"));
            var explicitPropInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedExplicitProperty"));
            var getOnlyInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedGetOnlyProperty"));
            var setOnlyInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedSetOnlyProperty"));

            Assert.AreEqual(2, autoPropInstrumented.Count());
            Assert.AreEqual(2, explicitPropInstrumented.Count());
            Assert.AreEqual(1, getOnlyInstrumented.Count());
            Assert.AreEqual(1, setOnlyInstrumented.Count());

            Assert.AreEqual(1, methods.Count(x => x.Method.Name.EndsWith("OneParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Method.Name.EndsWith("ArrayParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Method.Name.EndsWith("MultiParameterFunction")));

            Assert.AreEqual(1, methods.Count(x => x.Method.Name == "InstrumentedFunction" && x.Method.DeclaringType == typeof(ClassLevelImplicitMarkup.Nested)));

            Assert.AreEqual(11, methods.Count());
        }

        [TestMethod]
        public void MarkupTest_OnlyMethodsMarkedInstrumentReturned()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(ExplicitMarkup));

            var staticCtors = result.Where(x => x.IsConstructor && x.Method.IsStatic);
            var ctors = result.Where(x => x.IsConstructor && !x.Method.IsStatic);

            var methods = result.Where(x => x.IsMethod);

            Assert.IsTrue(staticCtors.Any());
            Assert.AreEqual(2, ctors.Count());
            
            // Single string param constructor
            Assert.IsTrue
                (
                    ctors.Any
                    (
                        x => x.Method.GetParameters() != null
                        && x.Method.GetParameters().Count() == 1
                        && x.Method.GetParameters().Any
                        (
                            y => y.ParameterType == typeof(string)
                        )
                    )
                );

            // Void constructor
            Assert.IsTrue(ctors.Any(x => x.Method.GetParameters() == null || x.Method.GetParameters().Count() == 0));

            var autoPropInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedAutoProperty"));
            var explicitPropInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedExplicitProperty"));
            var getOnlyInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedGetOnlyProperty"));
            var setOnlyInstrumented = methods.Where(x => x.Method.Name.EndsWith("InstrumentedSetOnlyProperty"));

            Assert.AreEqual(2, autoPropInstrumented.Count());
            Assert.AreEqual(2, explicitPropInstrumented.Count());
            Assert.AreEqual(1, getOnlyInstrumented.Count());
            Assert.AreEqual(1, setOnlyInstrumented.Count());

            Assert.AreEqual(1, methods.Count(x => x.Method.Name.EndsWith("OneParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Method.Name.EndsWith("ArrayParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Method.Name.EndsWith("MultiParameterFunction")));

            Assert.AreEqual(1, methods.Count(x => x.Method.Name == "InstrumentedFunction" && x.Method.DeclaringType == typeof(ExplicitMarkup.Nested)));

            Assert.AreEqual(11, methods.Count());
        }
    }
}

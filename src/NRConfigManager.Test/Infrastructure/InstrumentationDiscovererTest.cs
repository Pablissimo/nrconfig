using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfigManager.Configuration;
using NRConfigManager.Infrastructure;
using NRConfigManager.Test.TestClasses;
using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Infrastructure
{
    [TestClass]
    public class InstrumentationDiscovererTest
    {
        [TestMethod]
        public void InstrumentAttribute_CanOverrideMetricName()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethodOrPropertyAccessor && x.Target.Name == "TestMethodDifferentMetricName").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Method-level override name", ofInterest.MetricName);
            Assert.AreEqual(Metric.Scoped, ofInterest.Metric);
        }

        [TestMethod]
        public void InstrumentAttribute_CanOverrideMetricButInheritMetricName()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethodOrPropertyAccessor && x.Target.Name == "TestMethodDifferentMetricInheritedMetricName").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Metric name", ofInterest.MetricName);
            Assert.AreEqual(Metric.Both, ofInterest.Metric);
        }

        [TestMethod]
        public void InstrumentAttribute_CanOverrideMetricAndMetricName()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethodOrPropertyAccessor && x.Target.Name == "TestMethodDifferentMetricAndMetricName").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Method-level override name", ofInterest.MetricName);
            Assert.AreEqual(Metric.None, ofInterest.Metric);
        }

        [TestMethod]
        public void InstrumentAttribute_CanOverrideForNestedTypes()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(MixedMarkup));

            var ofInterest = result.Where(x => x.IsMethodOrPropertyAccessor && x.Target.Name == "OverrideMetricNameInNestedClassByDefault").FirstOrDefault();

            Assert.IsNotNull(ofInterest);
            Assert.AreEqual("Override class-level metric name", ofInterest.MetricName);
            Assert.AreEqual(Metric.Scoped, ofInterest.Metric);
        }

        [TestMethod]
        public void MarkupTest_AllMethodsInClassesMarkedInstrumentReturned()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(ClassLevelImplicitMarkup));

            var staticCtors = result.Where(x => x.IsConstructor && x.Target.IsStatic);
            var ctors = result.Where(x => x.IsConstructor && !x.Target.IsStatic);

            var methods = result.Where(x => x.IsMethodOrPropertyAccessor);

            Assert.IsTrue(staticCtors.Any());
            Assert.AreEqual(4, ctors.Count());
            Assert.AreEqual(1, ctors.Count(x => x.Target.DeclaringType == typeof(ClassLevelImplicitMarkup.Nested)));

            // Single string param constructor
            Assert.IsTrue
                (
                    ctors.Any
                    (
                        x => x.Target.GetParameters() != null
                        && x.Target.GetParameters().Count() == 1
                        && x.Target.GetParameters().Any
                        (
                            y => y.ParameterType == typeof(string)
                        )
                    )
                );

            // Void constructor
            Assert.IsTrue(ctors.Any(x => x.Target.GetParameters() == null || x.Target.GetParameters().Count() == 0));

            var autoPropInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedAutoProperty"));
            var explicitPropInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedExplicitProperty"));
            var getOnlyInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedGetOnlyProperty"));
            var setOnlyInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedSetOnlyProperty"));

            Assert.AreEqual(2, autoPropInstrumented.Count());
            Assert.AreEqual(2, explicitPropInstrumented.Count());
            Assert.AreEqual(1, getOnlyInstrumented.Count());
            Assert.AreEqual(1, setOnlyInstrumented.Count());

            Assert.AreEqual(1, methods.Count(x => x.Target.Name.EndsWith("OneParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Target.Name.EndsWith("ArrayParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Target.Name.EndsWith("MultiParameterFunction")));

            Assert.AreEqual(1, methods.Count(x => x.Target.Name == "InstrumentedFunction" && x.Target.DeclaringType == typeof(ClassLevelImplicitMarkup.Nested)));

            Assert.AreEqual(11, methods.Count());
        }

        [TestMethod]
        public void MarkupTest_OnlyMethodsMarkedInstrumentReturned()
        {
            var result = InstrumentationDiscoverer.GetInstrumentationSet(typeof(ExplicitMarkup));

            var staticCtors = result.Where(x => x.IsConstructor && x.Target.IsStatic);
            var ctors = result.Where(x => x.IsConstructor && !x.Target.IsStatic);

            var methods = result.Where(x => x.IsMethodOrPropertyAccessor);

            Assert.IsTrue(staticCtors.Any());
            Assert.AreEqual(2, ctors.Count());
            
            // Single string param constructor
            Assert.IsTrue
                (
                    ctors.Any
                    (
                        x => x.Target.GetParameters() != null
                        && x.Target.GetParameters().Count() == 1
                        && x.Target.GetParameters().Any
                        (
                            y => y.ParameterType == typeof(string)
                        )
                    )
                );

            // Void constructor
            Assert.IsTrue(ctors.Any(x => x.Target.GetParameters() == null || x.Target.GetParameters().Count() == 0));

            var autoPropInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedAutoProperty"));
            var explicitPropInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedExplicitProperty"));
            var getOnlyInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedGetOnlyProperty"));
            var setOnlyInstrumented = methods.Where(x => x.Target.Name.EndsWith("InstrumentedSetOnlyProperty"));

            Assert.AreEqual(2, autoPropInstrumented.Count());
            Assert.AreEqual(2, explicitPropInstrumented.Count());
            Assert.AreEqual(1, getOnlyInstrumented.Count());
            Assert.AreEqual(1, setOnlyInstrumented.Count());

            Assert.AreEqual(1, methods.Count(x => x.Target.Name.EndsWith("OneParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Target.Name.EndsWith("ArrayParameterFunction")));
            Assert.AreEqual(1, methods.Count(x => x.Target.Name.EndsWith("MultiParameterFunction")));

            Assert.AreEqual(1, methods.Count(x => x.Target.Name == "InstrumentedFunction" && x.Target.DeclaringType == typeof(ExplicitMarkup.Nested)));

            Assert.AreEqual(11, methods.Count());
        }

        [TestMethod]
        public void InstrumentationTest_ClassesWithHiddenMethodsWorkCorrectly()
        {
            var context = new InstrumentAttribute() { Scopes = InstrumentationScopes.PublicMethods };
            var targets = InstrumentationDiscoverer.GetInstrumentationSet(typeof(HidingMethodsExtended), context);

            // Expecting a single target
            Assert.AreEqual(1, targets.Count());
            Assert.AreEqual(typeof(HidingMethodsExtended), targets.First().Target.DeclaringType);
        }

        [TestMethod]
        public void InstrumentationTest_ClassesWithHiddenPropertiesWorkCorrectly()
        {
            var context = new InstrumentAttribute() { Scopes = InstrumentationScopes.Properties };
            var targets = InstrumentationDiscoverer.GetInstrumentationSet(typeof(HidingPropertiesExtended), context);

            // Expecting a single target
            Assert.AreEqual(2, targets.Count());
            Assert.AreEqual(typeof(HidingPropertiesExtended), targets.First().Target.DeclaringType);
        }

        [TestMethod]
        public void InstrumentationTest_IndexerPropertyOverloadsWorkCorrectly()
        {
            var context = new InstrumentAttribute() { Scopes = InstrumentationScopes.Properties };
            var targets = InstrumentationDiscoverer.GetInstrumentationSet(typeof(IndexerProperties), context);

            // Expecting a single target
            Assert.AreEqual(4, targets.Count());
            Assert.AreEqual(typeof(IndexerProperties), targets.First().Target.DeclaringType);
        }

        [TestMethod]
        public void InstrumentationTest_HiddenIndexerPropertyOverloadsWorkCorrectly()
        {
            var context = new InstrumentAttribute() { Scopes = InstrumentationScopes.Properties };
            var targets = InstrumentationDiscoverer.GetInstrumentationSet(typeof(IndexerPropertiesExtended), context);

            // Expecting a single target
            Assert.AreEqual(4, targets.Count());
            Assert.AreEqual(typeof(IndexerPropertiesExtended), targets.First().Target.DeclaringType);
        }

        [TestMethod]
        public void InstrumentationTest_CompilerGeneratedClasses_IgnoredByDefault()
        {
            var context = new InstrumentAttribute();
            var targets = InstrumentationDiscoverer.GetInstrumentationSet(typeof(ClassLevelImplicitMarkup), context);

            Assert.IsFalse(targets.Any(x => x.Target.Name == "CompilerGeneratedMethod"));
        }

        [TestMethod]
        public void InstrumentationTest_CompilerGeneratedClasses_IncludedIfExplicitlyRequested()
        {
            var context = new InstrumentAttribute() { IncludeCompilerGeneratedCode = true };
            var targets = InstrumentationDiscoverer.GetInstrumentationSet(typeof(ClassLevelImplicitMarkup), context);

            Assert.IsTrue(targets.Any(x => x.Target.Name == "CompilerGeneratedMethod"));
        }

        [TestMethod]
        public void InstrumentationTest_CompilerGeneratedMethods_IncludedIfRequestedAtEnclosingClassLevel()
        {
            // Explicit requirement here will be overruled by the class-level Instrument attribute
            var context = new InstrumentAttribute() { Scopes = InstrumentationScopes.PublicMethods, IncludeCompilerGeneratedCode = false };
            var targets = InstrumentationDiscoverer.GetInstrumentationSet(typeof(ExplicitMarkupCompilerGeneratedClass), context);

            Assert.IsTrue(targets.Any(x => x.Target.Name == "CompilerGeneratedMethod"));
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfigManager.Rendering;
using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Rendering
{
    [TestClass]
    public class ExtensionTest
    {
        [TestMethod]
        public void Merge_DoesNotDuplicateMethodMatchers_WhenContextSame()
        {
            TracerFactory first = new TracerFactory();
            Match firstMatch = new Match() { AssemblyName = "Test", ClassName = "TestClass" };
            firstMatch.Matches.Add(new ExactMethodMatcher("TestMethod1", new[] { "ParamType1", "ParamType2" }));
            first.MatchDefinitions.Add(firstMatch);

            TracerFactory second = new TracerFactory();
            Match secondMatch = new Match() { AssemblyName = firstMatch.AssemblyName, ClassName = firstMatch.ClassName };
            secondMatch.Matches.Add(new ExactMethodMatcher("TestMethod1", new[] { "ParamType1", "ParamType2" }));
            second.MatchDefinitions.Add(secondMatch);

            Extension firstExtension = new Extension();
            firstExtension.Instrumentation.TracerFactories.Add(first);

            Extension secondExtension = new Extension();
            secondExtension.Instrumentation.TracerFactories.Add(second);

            Extension merged = Extension.Merge(firstExtension, secondExtension);

            Assert.AreEqual(1, merged.Instrumentation.TracerFactories.Count());
            Assert.IsTrue(string.IsNullOrWhiteSpace(merged.Instrumentation.TracerFactories.First().MetricName));
            Assert.AreEqual(Metric.Unspecified, merged.Instrumentation.TracerFactories.First().Metric);
            Assert.AreEqual(1, merged.Instrumentation.TracerFactories.First().MatchDefinitions.Count);

            var firstMergedMatch = merged.Instrumentation.TracerFactories.First().MatchDefinitions.First();
            Assert.AreEqual("Test", firstMergedMatch.AssemblyName);
            Assert.AreEqual("TestClass", firstMergedMatch.ClassName);
            Assert.AreEqual(1, firstMergedMatch.Matches.Count);

            Assert.AreEqual("TestMethod1", firstMergedMatch.Matches.First().MethodName);
            Assert.AreEqual("ParamType1,ParamType2", string.Join(",", firstMergedMatch.Matches.First().ParameterTypes));
        }

        [TestMethod]
        public void Merge_CombinesExactMethodsMatchers_WhenContextSame()
        {
            TracerFactory first = new TracerFactory("MetricName", Metric.Scoped);
            Match firstMatch = new Match() { AssemblyName = "Test", ClassName = "TestClass" };
            firstMatch.Matches.Add(new ExactMethodMatcher("TestMethod1", new[] { "ParamType1", "ParamType2" }));
            first.MatchDefinitions.Add(firstMatch);

            TracerFactory second = new TracerFactory("MetricName", Metric.Scoped);
            Match secondMatch = new Match() { AssemblyName = firstMatch.AssemblyName, ClassName = firstMatch.ClassName };
            secondMatch.Matches.Add(new ExactMethodMatcher("TestMethod1", new[] { "ParamType3" }));
            second.MatchDefinitions.Add(secondMatch);

            Extension firstExtension = new Extension();
            firstExtension.Instrumentation.TracerFactories.Add(first);

            Extension secondExtension = new Extension();
            secondExtension.Instrumentation.TracerFactories.Add(second);

            Extension merged = Extension.Merge(firstExtension, secondExtension);

            Assert.AreEqual(1, merged.Instrumentation.TracerFactories.Count);

            var firstFactory = merged.Instrumentation.TracerFactories.First();
            Assert.AreEqual("MetricName", firstFactory.MetricName);
            Assert.AreEqual(Metric.Scoped, firstFactory.Metric);
            Assert.IsNotNull(firstFactory.MatchDefinitions);

            var firstFactoryMatches = firstFactory.MatchDefinitions;
            Assert.AreEqual(1, firstFactoryMatches.Count());

            var firstFactoryMatch = firstFactoryMatches.First();
            Assert.AreEqual("Test", firstFactoryMatch.AssemblyName);
            Assert.AreEqual("TestClass", firstFactoryMatch.ClassName);

            var methodMatchers = firstFactoryMatch.Matches;
            Assert.AreEqual(2, methodMatchers.Count);

            Assert.AreEqual(2, methodMatchers.Count(x => x.MethodName == "TestMethod1"));
            Assert.AreEqual(1, methodMatchers.Count(x => x.MethodName == "TestMethod1" && x.ParameterTypes == "ParamType1,ParamType2"));
            Assert.AreEqual(1, methodMatchers.Count(x => x.MethodName == "TestMethod1" && x.ParameterTypes == "ParamType3"));
        }

        [TestMethod]
        public void Merge_CreatesNewFactories_WhenFactoryDefinitionsDifferent()
        {
            TracerFactory first = new TracerFactory();
            Match firstMatch = new Match() { AssemblyName = "Test", ClassName = "TestClass" };
            firstMatch.Matches.Add(new ExactMethodMatcher("TestMethod1", new[] { "ParamType1", "ParamType2" }));
            first.MatchDefinitions.Add(firstMatch);

            TracerFactory second = new TracerFactory("DifferentFactory", Metric.Scoped);
            Match secondMatch = new Match() { AssemblyName = firstMatch.AssemblyName, ClassName = firstMatch.ClassName };
            secondMatch.Matches.Add(new ExactMethodMatcher("TestMethod1", new[] { "ParamType1", "ParamType2" }));
            second.MatchDefinitions.Add(secondMatch);

            Extension firstExtension = new Extension();
            firstExtension.Instrumentation.TracerFactories.Add(first);

            Extension secondExtension = new Extension();
            secondExtension.Instrumentation.TracerFactories.Add(second);

            Extension merged = Extension.Merge(firstExtension, secondExtension);

            Assert.AreEqual(2, merged.Instrumentation.TracerFactories.Count);
            Assert.AreEqual(1, merged.Instrumentation.TracerFactories.Count(x => x.MetricName == "DifferentFactory" && x.Metric == Metric.Scoped));
            Assert.AreEqual(1, merged.Instrumentation.TracerFactories.Count(x => x.Metric == Metric.Unspecified));

            var different = merged.Instrumentation.TracerFactories.Where(x => x.MetricName == "DifferentFactory").First();
            var other = merged.Instrumentation.TracerFactories.Where(x => x != different).First();

            Assert.AreEqual(1, different.MatchDefinitions.Count);
            Assert.AreEqual(1, other.MatchDefinitions.Count);

            Assert.AreEqual("Test", different.MatchDefinitions.First().AssemblyName);
            Assert.AreEqual("Test", other.MatchDefinitions.First().AssemblyName);

            Assert.AreEqual("TestClass", different.MatchDefinitions.First().ClassName);
            Assert.AreEqual("TestClass", other.MatchDefinitions.First().ClassName);

            Assert.AreEqual(1, different.MatchDefinitions.First().Matches.Count());
            Assert.AreEqual(1, other.MatchDefinitions.First().Matches.Count());

            Assert.AreEqual(1, different.MatchDefinitions.First().Matches.Count(x => x.MethodName == "TestMethod1" && x.ParameterTypes == "ParamType1,ParamType2"));
            Assert.AreEqual(1, other.MatchDefinitions.First().Matches.Count(x => x.MethodName == "TestMethod1" && x.ParameterTypes == "ParamType1,ParamType2"));
        }
    }
}

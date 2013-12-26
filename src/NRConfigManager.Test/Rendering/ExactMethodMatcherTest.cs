using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfigManager.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Rendering
{
    [TestClass]
    public class ExactMethodMatcherTest
    {
        [TestMethod]
        public void Equals_ReturnsTrue_ForEquivalentObjects()
        {
            var first = new ExactMethodMatcher("Method 1", new[] { "Parameter 1", "Parameter 2" });
            var second = new ExactMethodMatcher("Method 1", new[] { "Parameter 1", "Parameter 2" });

            Assert.IsTrue(first.Equals(second));
        }

        [TestMethod]
        public void Equals_ReturnsFalse_IfMethodNamesDoNotMatch()
        {
            var first = new ExactMethodMatcher("Method 1", new[] { "Parameter 1", "Parameter 2" });
            var second = new ExactMethodMatcher("Method 2", new[] { "Parameter 1", "Parameter 2" });

            Assert.IsFalse(first.Equals(second));
        }

        [TestMethod]
        public void Equals_ReturnsFalse_IfParameterTypesDoNotMatch()
        {
            var first = new ExactMethodMatcher("Method 1", new[] { "Parameter 1", "Parameter 2" });
            var second = new ExactMethodMatcher("Method 1", new[] { "Parameter 3", "Parameter 2" });

            Assert.IsFalse(first.Equals(second));
        }

        [TestMethod]
        public void GetHashCode_ReturnsSameValue_ForEquivalentObjects()
        {
            var first = new ExactMethodMatcher("Method 1", new[] { "Parameter 1", "Parameter 2" });
            var second = new ExactMethodMatcher("Method 1", new[] { "Parameter 1", "Parameter 2" });

            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NRConfig;
using NRConfigManager.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.Infrastructure
{
    [TestClass]
    public class InstrumentationTargetTest
    {
        [TestMethod]
        public void Constructor_InitialisesPropertiesCorrectly()
        {
            var methodDetails = new DummyIMethodDetails();

            var target = new InstrumentationTarget(methodDetails, "Metric name", "Transaction name", "1", Metric.Scoped);

            Assert.AreSame(methodDetails, target.Target);
            Assert.AreEqual("Metric name", target.MetricName);
            Assert.AreEqual("Transaction name", target.Name);
            Assert.AreEqual("1", target.TransactionNamingPriority);
            Assert.AreEqual(Metric.Scoped, target.Metric);
        }

        class DummyIMethodDetails : IMethodDetails
        {
            #region Dummy implementation
            public bool ContainsGenericParameters
            {
                get { throw new NotImplementedException(); }
            }

            public ITypeDetails[] GenericArguments
            {
                get { throw new NotImplementedException(); }
            }

            public IParameterDetails[] Parameters
            {
                get { throw new NotImplementedException(); }
            }

            public string Name
            {
                get { throw new NotImplementedException(); }
            }

            public ITypeDetails DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public InstrumentAttribute InstrumentationContext
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompilerGenerated
            {
                get { throw new NotImplementedException(); }
            }
            #endregion
        }
    }
}

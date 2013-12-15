using NRConfig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace NRConfigManager.Test.TestClasses
{
    [Instrument(IncludeCompilerGeneratedCode = false, Metric = Metric.Both, MetricName = "TestMetricName", Scopes = InstrumentationScopes.Methods | InstrumentationScopes.Constructors | InstrumentationScopes.Properties)]
    public class ClassLevelImplicitMarkup
    {
        public bool InstrumentedAutoProperty
        {
            get;
            set;
        }

        private string _instrumentedExplicitPropertyField;
        public string InstrumentedExplicitProperty
        {
            get { return _instrumentedExplicitPropertyField; }
            set { _instrumentedExplicitPropertyField = value; }
        }

        public string InstrumentedSetOnlyProperty
        {
            set { }
        }

        public bool InstrumentedGetOnlyProperty
        {
            get { return false; }
        }

        static ClassLevelImplicitMarkup()
        {

        }

        public ClassLevelImplicitMarkup()
        {

        }

        public ClassLevelImplicitMarkup(string stringParam)
        {

        }

        public ClassLevelImplicitMarkup(bool notInstrumented)
        {

        }

        public void VoidFunction()
        {

        }

        public void OneParameterFunction(string stringParam)
        {

        }
        
        public void ArrayParameterFunction(byte[] byteArrayParam)
        {

        }

        public void MultiParameterFunction(string stringParam, byte[] byteArrayParam)
        {

        }

        [CompilerGenerated]
        public void CompilerGeneratedMethod()
        {
        }

        public class Nested
        {
            public void InstrumentedFunction()
            {

            }
        }
    }
}

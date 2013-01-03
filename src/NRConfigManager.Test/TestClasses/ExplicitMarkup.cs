using NRConfig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.TestClasses
{
    public class ExplicitMarkup
    {
        [Instrument]
        public bool InstrumentedAutoProperty
        {
            get;
            set;
        }

        private string _instrumentedExplicitPropertyField;
        [Instrument]
        public string InstrumentedExplicitProperty
        {
            get { return _instrumentedExplicitPropertyField; }
            set { _instrumentedExplicitPropertyField = value; }
        }

        [Instrument]
        public string InstrumentedSetOnlyProperty
        {
            set { }
        }

        [Instrument]
        public bool InstrumentedGetOnlyProperty
        {
            get { return false; }
        }

        public int UninstrumentedAutoProperty
        {
            get;
            set;
        }

        private int _uninstrumentedExplicitPropertyField;
        public int UninstrumentedExplicitProperty
        {
            get { return _uninstrumentedExplicitPropertyField; }
            set { _uninstrumentedExplicitPropertyField = value; }
        }

        [Instrument]
        static ExplicitMarkup()
        {

        }

        [Instrument]
        public ExplicitMarkup()
        {

        }

        [Instrument]
        public ExplicitMarkup(string stringParam)
        {

        }

        public ExplicitMarkup(bool notInstrumented)
        {

        }

        [Instrument]
        public void VoidFunction()
        {

        }

        [Instrument]
        public void OneParameterFunction(string stringParam)
        {

        }
        
        [Instrument]
        public void ArrayParameterFunction(byte[] byteArrayParam)
        {

        }

        [Instrument]
        public void MultiParameterFunction(string stringParam, byte[] byteArrayParam)
        {

        }

        public void UninstrumentedFunction()
        {

        }

        public class Nested
        {
            [Instrument]
            public void InstrumentedFunction()
            {

            }

            public void UninstrumentedFunction()
            {

            }
        }
    }
}

using NRConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigManager.Test.TestClasses
{
    [Instrument]
    public class GenericsTest
    {
        public void OneParameterMethod<T>(T parameter1)
        {

        }

        public void TwoParameterMethod<T, U>(T parameter1, U parameter2)
        {

        }

        public void ThreeParameterMethod<T, U, V>(T parameter1, V parameter2)
        {

        }

        public void MixedMethod<T, U>(T parameter1, string stringParameter, U parameter3)
        {

        }

        public void GenericParametersMethod(IEnumerable<string> stringEnumerable)
        {

        }

        public void GenericParametersMethod(KeyValuePair<string, string> kvp)
        {

        }
    }
}

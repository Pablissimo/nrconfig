using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfiguration
{
    [Flags]
    public enum InstrumentationScopes
    {
        None = 0,
        PublicProperties = 1,
        NonPublicProperties = 2,
        Properties = PublicProperties | NonPublicProperties,
        
        PublicMethods = 4,
        NonPublicMethods = 8,
        Methods = PublicMethods | NonPublicMethods,

        PublicConstructors = 16,
        NonPublicConstructors = 32,
        Constructors = PublicConstructors | NonPublicConstructors,

        All = Properties | Methods | Constructors
    }
}

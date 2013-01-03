using System;
using System.Collections.Generic;
using System.Text;

namespace NewRelicConfiguration
{
    /// <summary>
    /// Specifies which instrumentation targets should be considered or rejected when producing
    /// a custom instrumentation XML file for New Relic. This is useful only at the class
    /// and assembly level.
    /// </summary>
    [Flags]
    public enum InstrumentationScopes
    {
        /// <summary>
        /// Nothing from this point forward should be instrumented.
        /// </summary>
        None = 0,
        /// <summary>
        /// Public property accessors should be instrumented.
        /// </summary>
        PublicProperties = 1,
        /// <summary>
        /// Non-public property accessors should be instrumented.
        /// </summary>
        NonPublicProperties = 2,
        /// <summary>
        /// All property accessors should be instrumented irrespective of
        /// visibility.
        /// </summary>
        Properties = PublicProperties | NonPublicProperties,
        
        /// <summary>
        /// Public methods should be instrumented.
        /// </summary>
        PublicMethods = 4,
        /// <summary>
        /// Non-public methods should be instrumented.
        /// </summary>
        NonPublicMethods = 8,
        /// <summary>
        /// All methods should be instrumented, irrespective of
        /// visibility.
        /// </summary>
        Methods = PublicMethods | NonPublicMethods,

        /// <summary>
        /// Public constructors should be instrumented.
        /// </summary>
        PublicConstructors = 16,
        /// <summary>
        /// Non-public methods should be instrumented.
        /// </summary>
        NonPublicConstructors = 32,
        /// <summary>
        /// All constructors should be instrumented, irrespective of
        /// visibility.
        /// </summary>
        Constructors = PublicConstructors | NonPublicConstructors,

        /// <summary>
        /// All properties, methods and constructors should be instrumented
        /// irrespective of visibility.
        /// </summary>
        All = Properties | Methods | Constructors
    }
}

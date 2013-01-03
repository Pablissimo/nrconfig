using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigBuilder
{
    internal enum OperationMode
    {
        /// <summary>
        /// Specifies that we should process assemblies into a custom instrumentation XML file.
        /// </summary>
        Create,
        /// <summary>
        /// Specifies that we should merge two or more custom instrumentation XML files into a 
        /// single file.
        /// </summary>
        Merge
    }
}

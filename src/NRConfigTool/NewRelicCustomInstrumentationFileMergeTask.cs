using log4net;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NRConfigTool.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigTool
{
    /// <summary>
    /// An MSBuild task that turns an input collection of New Relic custom instrumentation files into
    /// a single custom instrumentation file, removing duplicates in the process.
    /// </summary>
    public class NewRelicCustomInstrumentationFileMergeTask : AppDomainIsolatedTask
    {
        ILog _logger = LogManager.GetLogger(typeof(NewRelicCustomInstrumentationFileMergeTask));

        /// <summary>
        /// Gets or sets the collection of input files, each of which can contain wildcards if desired, to
        /// be merged into a single output file.
        /// </summary>
        [Required]
        public string[] InputFiles
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path of the output file to be generated.
        /// </summary>
        [Required]
        public string OutputFile
        {
            get;
            set;
        }

        public override bool Execute()
        {
            LogConfigurator.Configure(true, true, new BuildTaskLogAppender(this.Log));

            var inputFiles = PathHelper.GetMatchingPaths(this.InputFiles);

            var merger = new CustomInstrumentationMerger(inputFiles, this.OutputFile);
            merger.ContinueOnFailure = true;

            return merger.Execute();
        }
    }
}

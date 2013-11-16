using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NRConfigTool.Logging;
using log4net;

namespace NRConfigTool
{
    /// <summary>
    /// An MSBuild task that scans assemblies for classes or methods marked up with [Instrument] attributes
    /// and generates a New Relic custom instrumentation file based upon them.
    /// </summary>
    public class NewRelicCustomInstrumentationFileTask : AppDomainIsolatedTask
    {
        ILog _logger = LogManager.GetLogger(typeof(NewRelicCustomInstrumentationFileMergeTask));

        /// <summary>
        /// Gets or sets the collection if input files to be processed into a custom instrumentation file.
        /// </summary>
        [Required]
        public ITaskItem[] InputFiles { get; set; }

        /// <summary>
        /// Gets or sets the path to the output file to be generated. If absent, a file called CustomInstrumentation.xml
        /// is generated in the current directory.
        /// </summary>
        [Output]
        public ITaskItem OutputFile { get; set; }

        public override bool Execute()
        {
            LogConfigurator.Configure(true, true, new BuildTaskLogAppender(this.Log));

            var toReturn = true;

            if (this.InputFiles != null && this.InputFiles.Any())
            {
                IEnumerable<string> inputFiles = 
                    this
                    .InputFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.ItemSpec))
                    .Select(x => x.ItemSpec)
                    .ToList();

                string outputFile = "CustomInstrumentation.xml";
                if (this.OutputFile != null && !string.IsNullOrWhiteSpace(this.OutputFile.ItemSpec))
                {
                    outputFile = this.OutputFile.ItemSpec;
                }

                var generator = new CustomInstrumentationGenerator(inputFiles, outputFile);
                generator.ContinueOnFailure = true;

                try
                {
                    toReturn = generator.Execute();
                    this.OutputFile = new TaskItem(outputFile);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    toReturn = false;
                }
            }
            else
            {
                toReturn = false;
            }

            return toReturn;
        }
    }
}

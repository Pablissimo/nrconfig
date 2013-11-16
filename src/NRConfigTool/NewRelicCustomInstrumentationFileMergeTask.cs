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
    public class NewRelicCustomInstrumentationFileMergeTask : AppDomainIsolatedTask
    {
        ILog _logger = LogManager.GetLogger(typeof(NewRelicCustomInstrumentationFileMergeTask));

        [Required]
        public string[] InputFiles
        {
            get;
            set;
        }

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

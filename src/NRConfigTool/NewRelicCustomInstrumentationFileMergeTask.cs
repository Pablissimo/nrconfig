using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigTool
{
    public class NewRelicCustomInstrumentationFileMergeTask : AppDomainIsolatedTask
    {
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
            var inputFiles = PathHelper.GetMatchingPaths(this.InputFiles);

            var merger = new CustomInstrumentationMerger(inputFiles, this.OutputFile);
            merger.ContinueOnFailure = true;

            return merger.Execute();
        }
    }
}

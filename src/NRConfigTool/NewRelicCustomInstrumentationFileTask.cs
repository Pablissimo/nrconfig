using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NRConfigTool
{
    public class NewRelicCustomInstrumentationFileTask : AppDomainIsolatedTask
    {
        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Output]
        public ITaskItem OutputFile { get; set; }

        public override bool Execute()
        {
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
                    Log.LogErrorFromException(ex);
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

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
        public string InputFilter
        {
            get;
            set;
        }

        [Output]
        public string OutputFile
        {
            get;
            set;
        }

        public override bool Execute()
        {
            this.Log.LogMessage(MessageImportance.High, string.Format("Filter presented: {0}", this.InputFilter));

            return true;
        }
    }
}

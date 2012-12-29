using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigBuilder.Commands
{
    class BuildCommand : ConsoleCommand
    {
        public BuildCommand()
        {
            this.IsCommand("Build", "Builds a NewRelic custom instrumentation file from the specified assemblies");

            this.Options = new NDesk.Options.OptionSet()
            {

            };
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}

using BizArk.Core.CmdLine;
using NRConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigBuilder
{
    [CmdLineOptions(DefaultArgName = "InputFiles")]
    internal class CommandLineArgs : CmdLineObject
    {
        public CommandLineArgs()
        {
            this.OutputFile = "CustomInstrumentation.xml";
        }

        [CmdLineArg(Alias = "i", Required = true)]
        [Description("One or more paths to assemblies to be included in the custom instrumentation file. "
            + "Wildcards are permitted - for example, *.dll will process all DLL files in the current directory.")]
        public string[] InputFiles { get; set; }

        [CmdLineArg(Alias = "o", Required = false)]
        [Description("The path where the custom instrumentation file should be written. If not supplied, the file is "
            + "called CustomInstrumentation.xml and output to the current directory")]
        public string OutputFile { get; set; }

        [CmdLineArg(Alias = "m", Required = false)]
        [Description("Indicates that the tool should merge two or more custom instrumentation XML files into a single "
            + "output file.")]
        public bool MergeInputs { get; set; }

        [CmdLineArg(Alias = "v", Required = false)]
        [Description("Indicates that verbose diagnostic output should be rendered during operation")]
        public bool Verbose { get; set; }

        [CmdLineArg(Alias = "debug", Required = false)]
        [Description("Indicates that very detailed diagnostic output should be rendered during operation. Note - this will "
            + "slow down operation.")]
        public bool VeryVerbose { get; set; }

        [CmdLineArg(Alias = "c", Required = false)]
        [Description("Indicates that the process should try to continue even in the event of a failure")]
        public bool ContinueOnFailure { get; set; }

        [CmdLineArg(Alias = "w", Required = false)]
        [Description("Specifies one or more additional filters on fully-qualified type names to be included from the "
            + "assemblies to be processed - valid only with the /f flag. For example, MyProject.Repositories.* will match "
            + "all classes in the MyProject.Repositories namespace, while MyProject.MyType will match a single type.")]
        public string[] WhereTypeFullNameLike { get; set; }

        [CmdLineArg(Alias = "f", Required = false)]
        [Description("Generates a custom instrumentation file for the specified assemblies even if they haven't been annotated with "
            + "Instrument attributes. Specify a combination of items from the set {all, properties, methods, constructors}, and indicate "
            + "whether public or non-public items should be included by appending + or -.\n\n"
            + "For example:\n\n/f methods+\n\nwould instrument all public methods, while"
            + "\n\n/f properties+ methods+-\n\nwould instrument all public properties and all methods, public and private.\n\n"
            + "Note: If you don't specify a + or - after an item, it is assumed you only want to instrument public items.\n\n"
            + "The /f flag is equivalent to adding an assembly-level Instrument attribute in code and can be overridden by class or method-level "
            + "Instrument attributes as normal.")]
        public string[] ForceIfNotMarkedUp { get; set; }

        [CmdLineArg(Alias = "g", Required = false)]
        [Description("Specifies that code marked with the CompilerGenerated attribute, normally ignored, should "
            + "be included when detecting instrumentable methods. This defaults to off to avoid instrumenting the many "
            + "anonymous types that are generated when dealing with lambda expressions.")]
        public bool? IncludeCompilerGeneratedCode { get; set; }

        public bool ForceIfNotMarkedUpValid
        {
            get
            {
                string[] valid = new[] { "all", "properties", "methods", "constructors" };
                return
                    this.ForceIfNotMarkedUp != null
                    && this.ForceIfNotMarkedUp.All
                    (
                        x => valid.Any(v => v == x.ToLowerInvariant().Trim('+', '-'))
                    );
            }
        }

        public InstrumentationScopes ForceIfNotMarkedUpValidScopes
        {
            get
            {
                // Parse the force if not marked up valid property
                InstrumentationScopes scopes = InstrumentationScopes.None;
                foreach (string s in this.ForceIfNotMarkedUp.Select(x => x.ToLowerInvariant()))
                {
                    if (s == "all")
                    {
                        scopes = InstrumentationScopes.PublicConstructors | InstrumentationScopes.PublicMethods | InstrumentationScopes.PublicProperties;
                        break;
                    }

                    InstrumentationScopes parsed = InstrumentationScopes.None;
                    string remainder = null;
                    if (Enum.TryParse<InstrumentationScopes>(s.Trim('+', '-'), true, out parsed))
                    {
                        remainder = s.Substring(parsed.ToString().Length);
                    }

                    if (!string.IsNullOrWhiteSpace(remainder))
                    {
                        bool pub = false, priv = false;

                        if (remainder.Contains('+'))
                        {
                            pub = true;
                        }
                        if (remainder.Contains('-'))
                        {
                            priv = true;
                        }

                        string toParse = parsed.ToString();
                        if (pub && priv)
                        {
                            // Nothing to do here
                        }
                        else if (pub)
                        {
                            toParse = "Public" + toParse;
                        }
                        else if (priv)
                        {
                            toParse = "NonPublic" + toParse;
                        }

                        if (Enum.TryParse<InstrumentationScopes>(toParse, true, out parsed))
                        {
                            scopes |= parsed;
                        }
                    }
                    else
                    {
                        // We really want just the public version
                        string toParse = string.Format("Public{0}", parsed.ToString());

                        if (Enum.TryParse<InstrumentationScopes>(toParse, true, out parsed))
                        {
                            scopes |= parsed;
                        }
                    }
                }

                return scopes;
            }
        }
    }
}

using BizArk.Core.CmdLine;
using NewRelicConfigManager.Configuration;
using NewRelicConfigManager.Infrastructure;
using NewRelicConfigManager.Rendering;
using NewRelicConfigManager.Test.TestClasses;
using NewRelicConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NewRelicConfigBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs = new CommandLineArgs();
            parsedArgs.Initialize();
            if (parsedArgs.Help || !parsedArgs.IsValid())
            {
                Console.WriteLine(parsedArgs.GetHelpText(Console.WindowWidth));
                return;
            }

            // Validate the output filename
            try
            {
                var pathPart = Path.GetDirectoryName(parsedArgs.OutputFile);
                var filePart = Path.GetFileName(parsedArgs.OutputFile);

                var invalidFilenameCharacters = Path.GetInvalidFileNameChars();
                var invalidPathCharacters = Path.GetInvalidPathChars().Concat(invalidFilenameCharacters); // Because invalid path characters doesn't include *...

                if (pathPart.Any(x => invalidPathCharacters.Any(y => y == x)) || filePart.Any(x => invalidFilenameCharacters.Any(y => y == x)))
                {
                    Console.WriteLine("The specified output filename is invalid");
                    Console.WriteLine(parsedArgs.GetHelpText(Console.WindowWidth));
                }
            }
            catch
            {
                Console.WriteLine("The specified output filename is invalid");
                Console.WriteLine(parsedArgs.GetHelpText(Console.WindowWidth));
            }

            DateTime start = DateTime.Now;

            int exitCode = Environment.ExitCode = Process(parsedArgs);
            if (exitCode == 0)
            {
                Console.WriteLine("Output written to {0} in {1:f2}s", parsedArgs.OutputFile, (DateTime.Now - start).TotalSeconds);
            }
        }

        public static int Process(CommandLineArgs args)
        {
            try
            {
                List<string> inputPaths = new List<string>();
                foreach (string fileSpec in args.InputFiles)
                {
                    try
                    {
                        var spec = System.Environment.ExpandEnvironmentVariables(fileSpec);

                        var directory = Path.GetDirectoryName(spec);
                        if (string.IsNullOrWhiteSpace(directory))
                        {
                            directory = Environment.CurrentDirectory;
                        }

                        var filename = Path.GetFileName(spec);
                        inputPaths.AddRange(Directory.GetFiles(directory, filename));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed processing input file {0}", fileSpec, args.Verbose ? ex.ToString() : ex.Message);
                        if (!args.ContinueOnFailure)
                        {
                            return -1;
                        }
                    }
                }

                InstrumentAttribute assemblyAttribute = null;
                if (args.ForceIfNotMarkedUpValid)
                {
                    assemblyAttribute = new InstrumentAttribute() { Scopes = args.ForceIfNotMarkedUpValidScopes };
                }

                List<InstrumentationTarget> targets = new List<InstrumentationTarget>();

                foreach (string assyPath in inputPaths)
                {
                    Assembly assy = null;
                    try
                    {
                        assy = Assembly.LoadFrom(assyPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to load assembly from {0}: {1}", assyPath, args.Verbose ? ex.ToString() : ex.Message);
                        if (!args.ContinueOnFailure)
                        {
                            return -1;
                        }
                    }

                    targets.AddRange(InstrumentationDiscoverer.GetInstrumentationSet(assy, assemblyAttribute));
                }

                string tempPath = Path.GetTempFileName();
                using (FileStream w = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    new Renderer().RenderToStream(targets, w);
                }

                // Delete existing file, if required
                if (File.Exists(args.OutputFile))
                {
                    File.Delete(args.OutputFile);
                }

                File.Move(tempPath, args.OutputFile);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to process instrumentation: {0}", args.Verbose ? ex.ToString() : ex.Message);
                return -1;
            }
        }

        [CmdLineOptions(DefaultArgName="InputFiles")]
        public class CommandLineArgs : CmdLineObject
        {
            public CommandLineArgs()
            {
                this.OutputFile = "CustomInstrumentation.xml";
            }

            [CmdLineArg(Alias="i", Required = true)]
            [Description("One or more paths to assemblies to be included in the custom instrumentation file. "
                + "Wildcards are permitted - for example, *.dll will process all DLL files in the current directory.")]
            public string[] InputFiles { get; set; }
            [CmdLineArg(Alias="o", Required = false)]
            [Description("The path where the custom instrumentation file should be written. If not supplied, the file is "
                + "called CustomInstrumentation.xml and output to the current directory")]
            public string OutputFile { get; set; }

            [CmdLineArg(Alias="v", Required = false)]
            [Description("Indicates that verbose diagnostic output should be rendered during operation")]
            public bool Verbose { get; set; }

            [CmdLineArg(Alias = "c", Required = false)]
            [Description("Indicates that the process should try to continue even in the event of a failure")]
            public bool ContinueOnFailure { get; set; }

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

            public bool ForceIfNotMarkedUpValid
            {
                get
                {
                    string[] valid = new[]{ "all", "properties", "methods", "constructors" };
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
}

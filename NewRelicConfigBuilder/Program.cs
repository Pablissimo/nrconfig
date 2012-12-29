using BizArk.Core.CmdLine;
using NewRelicConfigBuilder.Commands;
using NewRelicConfigManager.Configuration;
using NewRelicConfigManager.Infrastructure;
using NewRelicConfigManager.Rendering;
using NewRelicConfigManager.Test.TestClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            int exitCode = Environment.ExitCode = Process(parsedArgs.InputFiles, parsedArgs.OutputFile, parsedArgs.Verbose, parsedArgs.ForceContinueOnFailure);
            if (exitCode == 0)
            {
                Console.WriteLine("Output written to {0}", parsedArgs.OutputFile);
            }
        }

        public static int Process(IEnumerable<string> fileSpecs, string outputPath, bool verbose, bool continueOnFailure)
        {
            try
            {
                List<string> inputPaths = new List<string>();
                foreach (string fileSpec in fileSpecs)
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
                        Console.WriteLine("Failed processing input file {0}", fileSpec, verbose ? ex.ToString() : ex.Message);
                        if (!continueOnFailure)
                        {
                            return -1;
                        }
                    }
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
                        Console.WriteLine("Failed to load assembly from {0}: {1}", assyPath, verbose ? ex.ToString() : ex.Message);
                        if (!continueOnFailure)
                        {
                            return -1;
                        }
                    }

                    targets.AddRange(InstrumentationDiscoverer.GetInstrumentationSet(assy));
                }

                string tempPath = Path.GetTempFileName();
                using (FileStream w = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    new Renderer().RenderToStream(targets, w);
                }

                // Delete existing file, if required
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                File.Move(tempPath, outputPath);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to process instrumentation: {0}", verbose ? ex.ToString() : ex.Message);
                return -1;
            }
        }

        [CmdLineOptions(DefaultArgName="InputFiles")]
        class CommandLineArgs : CmdLineObject
        {
            public CommandLineArgs()
            {
                this.OutputFile = "CustomInstrumentation.xml";
            }

            [CmdLineArg(Alias="i", Required=true)]
            [Description("One or more paths to assemblies to be included in the custom instrumentation file. "
                + "Wildcards are permitted - for example, *.dll will process all DLL files in the current directory.")]
            public string[] InputFiles { get; set; }
            [CmdLineArg(Alias="o", Required=false)]
            [Description("The path where the custom instrumentation file should be written. If not supplied, the file is "
                + "called CustomInstrumentation.xml and output to the current directory")]
            public string OutputFile { get; set; }

            [CmdLineArg(Alias="v", Required=false)]
            [Description("Indicates that verbose diagnostic output should be rendered during operation")]
            public bool Verbose { get; set; }

            [CmdLineArg(Alias = "f", Required = false)]
            [Description("Indicates that the process should try to continue even in the event of a failure")]
            public bool ForceContinueOnFailure { get; set; }
        }
    }
}

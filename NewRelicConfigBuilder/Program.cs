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
        const int VERBOSITY_NORMAL = 0;
        const int VERBOSITY_VERBOSE = 1;
        const int VERBOSITY_DEBUG = 2;

        static void Main(string[] args)
        {
            var parsedArgs = new CommandLineArgs();
            parsedArgs.Initialize();
            if (parsedArgs.Help || !parsedArgs.IsValid())
            {
                Console.WriteLine(parsedArgs.GetHelpText(Console.WindowWidth));
                return;
            }

            int verbosity = VERBOSITY_NORMAL;
            if (parsedArgs.VeryVerbose)
            {
                verbosity = VERBOSITY_DEBUG;
            }
            else if (parsedArgs.Verbose)
            {
                verbosity = VERBOSITY_NORMAL;
            }

            ConfigureLogging(verbosity);

            var mode = OperationMode.Create;
            if (parsedArgs.MergeInputs)
            {
                mode = OperationMode.Merge;
            }

            // Validate the output filename
            try
            {
                var pathPart = Path.GetDirectoryName(parsedArgs.OutputFile);
                var filePart = Path.GetFileName(parsedArgs.OutputFile);

                var invalidFilenameCharacters = Path.GetInvalidFileNameChars();
                var invalidPathCharacters = Path.GetInvalidPathChars().Concat(new[] { '*', '?' }); // Because invalid path characters doesn't include *...

                if (pathPart.Any(x => invalidPathCharacters.Any(y => y == x)) || filePart.Any(x => invalidFilenameCharacters.Any(y => y == x)))
                {
                    Console.WriteLine("The specified output filename is invalid : " + parsedArgs.OutputFile + " " + pathPart + " " + filePart);
                    Console.WriteLine(parsedArgs.GetHelpText(Console.WindowWidth));

                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The specified output filename is invalid ({0})", ex.Message);
                Console.WriteLine(parsedArgs.GetHelpText(Console.WindowWidth));

                return;
            }

            DateTime start = DateTime.Now;

            int exitCode = 0;
            switch (mode)
            {
                case OperationMode.Create:
                    exitCode = Environment.ExitCode = ProcessCreate(parsedArgs);
                    break;
                case OperationMode.Merge:
                    exitCode = Environment.ExitCode = ProcessMerge(parsedArgs);
                    break;
            }

            if (exitCode == 0)
            {
                Console.WriteLine("Output written to {0} in {1:f2}s", parsedArgs.OutputFile, (DateTime.Now - start).TotalSeconds);
            }
        }

        private static void ConfigureLogging(int verbosity)
        {
            log4net.Config.BasicConfigurator.Configure();

            var hierarchy = log4net.LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;

            if (verbosity <= VERBOSITY_NORMAL)
            {
                hierarchy.Root.Level = log4net.Core.Level.Off;
            }
            else
            {
                switch (verbosity)
                {
                    case VERBOSITY_VERBOSE:
                        hierarchy.Root.Level = log4net.Core.Level.Info;
                        break;
                    case VERBOSITY_DEBUG:
                        hierarchy.Root.Level = log4net.Core.Level.Debug;
                        break;
                }
            }
        }

        private static IEnumerable<string> GetInputFiles(CommandLineArgs args)
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
                    ErrorOut(string.Format("Failed processing input file {0} : {{0}}", fileSpec), ex, args.Verbose || args.VeryVerbose);
                    if (!args.ContinueOnFailure)
                    {
                        throw;
                    }
                }
            }

            return inputPaths;
        }

        public static int ProcessMerge(CommandLineArgs args)
        {
            try
            {
                IEnumerable<string> inputPaths = GetInputFiles(args);

                List<Extension> extensions = new List<Extension>();
                foreach (string path in inputPaths)
                {
                    try
                    {
                        using (FileStream r = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            extensions.Add(Renderer.LoadRenderedFromStream(r));
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorOut(string.Format("Failed to load file {0}: {{0}}", path), ex, args.Verbose || args.VeryVerbose);
                        if (!args.ContinueOnFailure)
                        {
                            throw;
                        }
                    }

                    Extension merged = Extension.Merge(extensions.ToArray());

                    string tempPath = Path.GetTempFileName();
                    using (FileStream w = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        Renderer.RenderToStream(merged, w);
                    }

                    // Delete existing file, if required
                    if (File.Exists(args.OutputFile))
                    {
                        File.Delete(args.OutputFile);
                    }

                    File.Move(tempPath, args.OutputFile);
                }
            }
            catch (Exception ex)
            {
                ErrorOut("Failed to process instrumentation: {0}", ex, args.Verbose || args.VeryVerbose);
                return -1;
            }

            return 0;
        }

        public static int ProcessCreate(CommandLineArgs args)
        {
            try
            {
                IEnumerable<string> inputPaths = GetInputFiles(args);

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
                        ErrorOut(string.Format("Failed to load assembly from {0}: {{0}}", assyPath), ex, args.Verbose || args.VeryVerbose);
                        if (!args.ContinueOnFailure)
                        {
                            return -1;
                        }
                    }

                    var toAdd = InstrumentationDiscoverer.GetInstrumentationSet(assy, assemblyAttribute);
                    if (args.Verbose)
                    {
                        Console.WriteLine("Processed {0} targets from {1}", toAdd.Count(), assy.FullName);
                    }

                    targets.AddRange(toAdd);
                }

                string tempPath = Path.GetTempFileName();
                using (FileStream w = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    Renderer.RenderToStream(targets, w);
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
                ErrorOut("Failed to process instrumentation: {0}", ex, args.Verbose || args.VeryVerbose);
                return -1;
            }
        }

        private static void ErrorOut(string message, Exception ex, bool verbose)
        {
            Console.WriteLine(message, verbose ? ex.ToString() : ex.Message);
        }
    }
}

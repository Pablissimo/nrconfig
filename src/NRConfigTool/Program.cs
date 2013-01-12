using BizArk.Core.CmdLine;
using NRConfigManager.Configuration;
using NRConfigManager.Infrastructure;
using NRConfigManager.Rendering;
using NRConfigManager.Test.TestClasses;
using NRConfig;
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
using System.Text.RegularExpressions;

namespace NewRelicConfigBuilder
{
    class Program
    {
        const int VERBOSITY_NORMAL = 0;
        const int VERBOSITY_VERBOSE = 1;
        const int VERBOSITY_DEBUG = 2;

        const int MAX_TARGETS_BEFORE_WARNING = 2000;

        const int RETURN_FAILURE = -1;
        const int RETURN_SUCCESS = 0;

        static int _windowWidth = 80;

        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var parsedArgs = new CommandLineArgs();
            parsedArgs.Initialize();
            if (parsedArgs.Help || !parsedArgs.IsValid())
            {
                try
                {
                    _windowWidth = Console.WindowWidth;
                }
                catch (IOException)
                {
                    // Running in powershell?
                }

                Console.WriteLine(parsedArgs.GetHelpText(_windowWidth));
                return Environment.ExitCode = RETURN_FAILURE;
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
                    Console.WriteLine(parsedArgs.GetHelpText(_windowWidth));

                    return Environment.ExitCode = RETURN_FAILURE;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The specified output filename is invalid ({0})", ex.Message);
                Console.WriteLine(parsedArgs.GetHelpText(_windowWidth));

                return Environment.ExitCode = RETURN_FAILURE;
            }

            DateTime start = DateTime.Now;
            long bytesBefore = GC.GetTotalMemory(true);

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

            long bytesAfter = GC.GetTotalMemory(false);

            if (exitCode == 0)
            {
                Console.WriteLine("Output written to {0} in {1:f2}s - memory allocated {2:n0}KB", parsedArgs.OutputFile, (DateTime.Now - start).TotalSeconds, (bytesAfter - bytesBefore) / 1024f);
            }

            return Environment.ExitCode;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log4net.LogManager.GetLogger(typeof(Program)).Fatal("Unhandled exception during operation - try running with /v or /debug flags for a trace to be sent to developer.", e.ExceptionObject as Exception);
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

                    Predicate<Type> filter = null;
                    if (args.WhereTypeFullNameLike != null && args.WhereTypeFullNameLike.Any())
                    {
                        filter = MakeFilterFromWildcards(args.WhereTypeFullNameLike);
                    }

                    var toAdd = InstrumentationDiscoverer.GetInstrumentationSet(assy, assemblyAttribute, filter);
                    if (args.Verbose)
                    {
                        Console.WriteLine("Processed {0} targets from {1}", toAdd.Count(), assy.FullName);
                    }

                    targets.AddRange(toAdd);
                }

                Console.WriteLine("Processed {0} targets", targets.Count);

                if (targets.Count > MAX_TARGETS_BEFORE_WARNING)
                {
                    Console.WriteLine("WARNING - New Relic recommend instrumenting no more than {0} targets to avoid performance issues.", MAX_TARGETS_BEFORE_WARNING);
                    Console.WriteLine("See https://newrelic.com/docs/dotnet/CustomInstrumentation.html for more information");
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

        private static Predicate<Type> MakeFilterFromWildcards(string[] filters)
        {
            List<Regex> regexes = new List<Regex>();
            foreach (var filter in filters)
            {
                // http://www.codeproject.com/Articles/11556/Converting-Wildcards-to-Regexes
                string safe = Regex.Escape(filter);
                regexes.Add(new Regex("^" + safe.Replace(@"\*", ".*").Replace(@"\?", ".?") + "$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled));
            }

            return delegate(Type t)
            {
                return regexes.Any(r => r.Match(t.FullName).Success);
            };
        }

        private static void ErrorOut(string message, Exception ex, bool verbose)
        {
            Console.WriteLine(message, verbose ? ex.ToString() : ex.Message);
        }
    }
}

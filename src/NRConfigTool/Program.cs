using log4net;
using NRConfigTool;
using NRConfigTool.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NewRelicConfigBuilder
{
    class Program
    {
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

            LogConfigurator.Configure(parsedArgs.Verbose || parsedArgs.VeryVerbose, parsedArgs.VeryVerbose);

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

            Console.ReadLine();

            return Environment.ExitCode;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log4net.LogManager.GetLogger(typeof(Program)).Fatal("Unhandled exception during operation - try running with /v or /debug flags for a trace to be sent to developer.", e.ExceptionObject as Exception);
        }

        private static IEnumerable<string> GetInputFiles(CommandLineArgs args)
        {
            return PathHelper.GetMatchingPaths(args.InputFiles, !args.ContinueOnFailure);
        }

        public static int ProcessMerge(CommandLineArgs args)
        {
            try
            {
                IEnumerable<string> inputPaths = GetInputFiles(args);
                string outputPath = "CustomInstrumentation.xml";

                if (!string.IsNullOrWhiteSpace(args.OutputFile))
                {
                    outputPath = args.OutputFile;
                }

                var merger = new CustomInstrumentationMerger(inputPaths, outputPath);
                merger.ContinueOnFailure = args.ContinueOnFailure;

                bool result = merger.Execute();

                return result ? 0 : -1;
            }
            catch (Exception ex)
            {
                ErrorOut("Failed to process instrumentation: {0}", ex, args.Verbose || args.VeryVerbose);
                return -1;
            }
        }

        public static int ProcessCreate(CommandLineArgs args)
        {
            try
            {
                IEnumerable<string> inputPaths = GetInputFiles(args);
                string outputPath = "CustomInstrumentation.xml";

                if (!string.IsNullOrWhiteSpace(args.OutputFile))
                {
                    outputPath = args.OutputFile;
                }

                var generator = new CustomInstrumentationGenerator(inputPaths, outputPath);
                if (args.ForceIfNotMarkedUpValid)
                {
                    generator.AutomaticInstrumentationScopes = args.ForceIfNotMarkedUpValidScopes;
                }

                generator.ContinueOnFailure = args.ContinueOnFailure;
                generator.IncludeCompilerGeneratedCode = args.IncludeCompilerGeneratedCode ?? false;

                bool result = generator.Execute();

                return result ? RETURN_SUCCESS : RETURN_FAILURE;
            }
            catch (Exception ex)
            {
                ErrorOut("Failed to process instrumentation: {0}", ex, args.Verbose || args.VeryVerbose);
                return RETURN_FAILURE;
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
            LogManager.GetLogger(typeof(Program)).Error(message, ex);
            Console.WriteLine(message, verbose ? ex.ToString() : ex.Message);
        }
    }
}

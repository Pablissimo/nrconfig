using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NRConfig;
using NRConfigManager.Configuration;
using NRConfigManager.Infrastructure;
using NRConfigManager.Rendering;
using log4net;

namespace NRConfigTool
{
    /// <summary>
    /// Processes an input set of .NET assemblies and generates a New Relic custom instrumentation
    /// file that includes all profilable methods detected.
    /// </summary>
    public class CustomInstrumentationGenerator
    {
        ILog _logger = LogManager.GetLogger(typeof(CustomInstrumentationGenerator));

        const int MAX_TARGETS_BEFORE_WARNING = 2000;

        /// <summary>
        /// Gets the set of paths to assemblies that should be processed.
        /// </summary>
        public IEnumerable<string> InputPaths { get; private set; }
        /// <summary>
        /// Gets the output path to be used. If absent, a file called CustomInstrumentation.xml is
        /// generated in the current directory.
        /// </summary>
        public string OutputPath { get; private set; }

        /// <summary>
        /// Gets or sets the InstrumentationScopes flag set describing which kinds of methods should
        /// be instrumented even if they lack [Instrument] attributes.
        /// </summary>
        public InstrumentationScopes AutomaticInstrumentationScopes { get; set; }
        /// <summary>
        /// Gets or sets whether to include methods from classes marked with the [CompilerGenerated] 
        /// attribute should be included in custom instrumentation if they match a scope described by
        /// AutomaticInstrumentationScopes.
        /// </summary>
        public bool IncludeCompilerGeneratedCode { get; set; }
        /// <summary>
        /// Gets or sets whether to abort processing on the first failure, or to instead make a best-effort
        /// run to generate an instrumentation file.
        /// </summary>
        public bool ContinueOnFailure { get; set; }

        /// <summary>
        /// Gets or sets a delegate that filters whether a type found in an assembly should be considered for
        /// instrumentation.
        /// </summary>
        public Predicate<Type> TypeFilter { get; set; }
                
        public CustomInstrumentationGenerator(IEnumerable<string> inputPaths, string outputPath)
        {
            this.InputPaths = inputPaths;
            this.OutputPath = outputPath;

            this.AutomaticInstrumentationScopes = InstrumentationScopes.None;
            this.TypeFilter = x => true;
        }

        public bool Execute()
        {
            InstrumentAttribute assemblyAttribute = null;
            if (this.AutomaticInstrumentationScopes != InstrumentationScopes.None)
            {
                assemblyAttribute = new InstrumentAttribute() { Scopes = this.AutomaticInstrumentationScopes };
                assemblyAttribute.IncludeCompilerGeneratedCode = this.IncludeCompilerGeneratedCode;
            }

            List<InstrumentationTarget> targets = new List<InstrumentationTarget>();

            foreach (string assyPath in this.InputPaths)
            {
                Assembly assy = null;
                try
                {
                    assy = Assembly.LoadFrom(assyPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Failed to load assembly from {0}", assyPath), ex);
                    if (!this.ContinueOnFailure)
                    {
                        return false;
                    }
                }

                try
                {
                    var toAdd = InstrumentationDiscoverer.GetInstrumentationSet(assy, assemblyAttribute, this.TypeFilter);
                    _logger.Info(string.Format("Processed {0} targets from {1}", toAdd.Count(), assy.FullName));

                    targets.AddRange(toAdd);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    _logger.Error(string.Format("Failed to get instrumentation set for {0}: {1}", assy.FullName, ex.Message), ex);
                    if (ex.LoaderExceptions != null && ex.LoaderExceptions.Any())
                    {
                        foreach (var loaderException in ex.LoaderExceptions)
                        {
                            _logger.Error("Loader exception (related to previous instrumentation set failure): " + loaderException.Message, loaderException);
                        }
                    }

                    if (!this.ContinueOnFailure)
                    {
                        return false;
                    }
                }
            }

            _logger.Info(string.Format("Processed {0} targets", targets.Count));

            if (targets.Count > MAX_TARGETS_BEFORE_WARNING)
            {
                _logger.Warn(string.Format("WARNING - New Relic recommend instrumenting no more than {0} targets to avoid performance issues.", MAX_TARGETS_BEFORE_WARNING));
                _logger.Warn(string.Format("See https://newrelic.com/docs/dotnet/CustomInstrumentation.html for more information"));
            }

            string tempPath = Path.GetTempFileName();
            using (FileStream w = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                Renderer.RenderToStream(targets, w);
            }

            // Delete existing file, if required
            if (File.Exists(this.OutputPath))
            {
                File.Delete(this.OutputPath);
            }

            File.Move(tempPath, this.OutputPath);

            return true;
        }
    }
}

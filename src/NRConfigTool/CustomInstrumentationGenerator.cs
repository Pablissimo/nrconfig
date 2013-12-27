using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NRConfig;
using NRConfigManager.Infrastructure;
using NRConfigManager.Rendering;
using log4net;
using NRConfigManager.Infrastructure.Cci;
using NRConfigManager.Infrastructure.Reflected;

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
        /// Gets or sets whether type and method discovery is performed by a legacy reflection method, which
        /// requires all dependencies to be available in the GAC or next to the assembly.
        /// </summary>
        public bool UseReflectionBasedDiscovery { get; set; }

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

            var targets = new List<InstrumentationTarget>();

            foreach (string assyPath in this.InputPaths)
            {
                InstrumentationDiscovererBase discoverer = null;
                
                if (this.UseReflectionBasedDiscovery)
                {
                    _logger.Info("Using legacy reflection-based discovery on request");
                    discoverer = new ReflectedInstrumentationDiscoverer();
                }
                else
                {
                    discoverer = new CciInstrumentationDiscoverer();
                }

                targets.AddRange(discoverer.GetInstrumentationSet(assyPath, assemblyAttribute, x => true));
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

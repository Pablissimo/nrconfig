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

namespace NRConfigTool
{
    public class CustomInstrumentationGenerator
    {
        const int MAX_TARGETS_BEFORE_WARNING = 2000;

        public IEnumerable<string> InputPaths { get; private set; }
        public string OutputPath { get; private set; }

        public InstrumentationScopes AutomaticInstrumentationScopes { get; set; }
        public bool IncludeCompilerGeneratedCode { get; set; }
        public bool ContinueOnFailure { get; set; }

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
                    this.LogError(string.Format("Failed to load assembly from {0}: {{0}}", assyPath), ex);
                    if (!this.ContinueOnFailure)
                    {
                        return false;
                    }
                }
                
                var toAdd = InstrumentationDiscoverer.GetInstrumentationSet(assy, assemblyAttribute, this.TypeFilter);
                this.LogMessage(string.Format("Processed {0} targets from {1}", toAdd.Count(), assy.FullName));

                targets.AddRange(toAdd);
            }

            this.LogMessage(string.Format("Processed {0} targets", targets.Count));

            if (targets.Count > MAX_TARGETS_BEFORE_WARNING)
            {
                this.LogMessage(string.Format("WARNING - New Relic recommend instrumenting no more than {0} targets to avoid performance issues.", MAX_TARGETS_BEFORE_WARNING));
                this.LogMessage(string.Format("See https://newrelic.com/docs/dotnet/CustomInstrumentation.html for more information"));
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

        protected virtual void LogError(string error, Exception ex)
        {
        }

        protected virtual void LogMessage(string message)
        {
        }

        protected virtual void LogTrace(string trace)
        {
        }
    }
}

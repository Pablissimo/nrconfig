using log4net;
using NRConfigManager.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigTool
{
    public class CustomInstrumentationMerger
    {
        ILog _logger = log4net.LogManager.GetLogger(typeof(CustomInstrumentationMerger));

        public IEnumerable<string> InputPaths { get; private set; }
        public string OutputPath { get; private set; }

        public bool ContinueOnFailure { get; set; }

        public CustomInstrumentationMerger(IEnumerable<string> inputPaths, string outputPath)
        {
            this.InputPaths = inputPaths;
            this.OutputPath = outputPath;
        }

        public bool Execute()
        {
            List<Extension> extensions = new List<Extension>();
            foreach (string path in this.InputPaths)
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
                    _logger.Error(string.Format("Failed to load file {0}: {1}", path, ex.Message), ex);

                    if (!this.ContinueOnFailure)
                    {
                        return false;
                    }
                }

                Extension merged = Extension.Merge(extensions.ToArray());

                string tempPath = Path.GetTempFileName();
                try
                {
                    using (FileStream w = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        Renderer.RenderToStream(merged, w);
                    }

                    // Delete existing file, if required
                    if (File.Exists(this.OutputPath))
                    {
                        File.Delete(this.OutputPath);
                    }

                    File.Move(tempPath, this.OutputPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Failed rendering merged custom instrumentation file: {0}", ex.Message));
                    return false;
                }
            }

            return true;
        }
    }
}

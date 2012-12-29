// using ManyConsole;
using NewRelicConfigManager.Infrastructure;
using NewRelicConfigManager.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewRelicConfigBuilder.Commands
{
    //class BuildCommand : ConsoleCommand
    //{
    //    List<string> _fileSpecs = new List<string>();
    //    string _outputPath = null;

    //    public BuildCommand()
    //    {
    //        this.IsCommand("Build", "Builds a NewRelic custom instrumentation file from the specified assemblies");

    //        this.HasRequiredOption("o|outputFile=", "Path to the desired custom instrumentation XML file. If the file already exists it will overwritten", s => { _outputPath = s; });
    //        this.HasRequiredOption("i|inputFile=", "Path to an assembly to be instrumented. If more than one assembly is to be instrumented, the -i option can be used multiple times. Standard filesystem wildcards are also permitted - for example, *.dll", s => _fileSpecs.Add(s));
    //    }

    //    public override int Run(string[] remainingArguments)
    //    {
    //        List<string> inputPaths = new List<string>();
    //        foreach (string fileSpec in _fileSpecs)
    //        {
    //            var spec = System.Environment.ExpandEnvironmentVariables(fileSpec);

    //            var directory = Path.GetDirectoryName(spec);
    //            if (string.IsNullOrWhiteSpace(directory))
    //            {
    //                directory = Environment.CurrentDirectory;
    //            }

    //            var filename = Path.GetFileName(spec);
    //            inputPaths.AddRange(Directory.GetFiles(directory, filename));
    //        }

    //        List<Assembly> assemblies = new List<Assembly>();
    //        foreach (string assyPath in inputPaths)
    //        {
    //            Assembly toAdd = null;
    //            try 
    //            {
    //                toAdd = Assembly.LoadFrom(assyPath);
    //            }
    //            catch { }

    //            assemblies.Add(toAdd);
    //        }

    //        string tempPath = Path.GetTempFileName();
    //        using (FileStream w = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
    //        {
    //            var instrumentationSet = InstrumentationDiscoverer.GetInstrumentationSet(assemblies);
    //            new Renderer().RenderToStream(instrumentationSet, w);
    //        }

    //        // Delete existing file, if required
    //        if (File.Exists(_outputPath))
    //        {
    //            File.Delete(_outputPath);
    //        }

    //        File.Move(tempPath, _outputPath);

    //        return 0;
    //    }
    //}
}

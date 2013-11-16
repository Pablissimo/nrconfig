using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigTool
{
    public static class PathHelper
    {
        public static IEnumerable<string> GetMatchingPaths(IEnumerable<string> pathsWithPossibleWildcards, bool throwOnFailures = false)
        {
            var logger = LogManager.GetLogger(typeof(PathHelper));

            List<string> toReturn = new List<string>();

            foreach (string fileSpec in pathsWithPossibleWildcards)
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
                    toReturn.AddRange(Directory.GetFiles(directory, filename));
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("Failed processing input file {0} : {{0}}", fileSpec), ex);
                    if (throwOnFailures)
                    {
                        throw;
                    }
                }
            }

            return toReturn.AsReadOnly();
        }
    }
}

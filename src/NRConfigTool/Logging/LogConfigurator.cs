using log4net.Appender;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigTool.Logging
{
    internal static class LogConfigurator
    {
        public static void Configure(bool verbose, bool debug, IAppender appender = null)
        {
            if (appender != null)
            {
                BasicConfigurator.Configure(appender);
            }
            else
            {
                // Just take the default
                BasicConfigurator.Configure();
            }

            var hierarchy = log4net.LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
            
            if (debug)
            {
                hierarchy.Root.Level = log4net.Core.Level.Debug;
            }
            else if (verbose)
            {
                hierarchy.Root.Level = log4net.Core.Level.Info;
            }
            else 
            {
                hierarchy.Root.Level = log4net.Core.Level.Error;
            }
        }
    }
}

using log4net.Core;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRConfigTool.Logging
{
    /// <summary>
    /// A log4net appender that directs logged messages to a TaskLoggingHelper instance
    /// as would be available to custom MSBuild tasks.
    /// </summary>
    internal class BuildTaskLogAppender : log4net.Appender.AppenderSkeleton
    {
        TaskLoggingHelper _taskLoggingHelper;

        public BuildTaskLogAppender(TaskLoggingHelper taskLoggingHelper)
        {
            if (taskLoggingHelper == null)
            {
                throw new ArgumentNullException("taskLoggingHelper");
            }

            _taskLoggingHelper = taskLoggingHelper;
        }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            switch (loggingEvent.Level.Name.ToLower())
            {
                case "alert":
                case "critical":
                case "emergency":
                case "error":
                case "fatal":
                case "severe":
                    _taskLoggingHelper.LogError(loggingEvent.RenderedMessage);
                    break;
                case "warn":
                    _taskLoggingHelper.LogWarning(loggingEvent.RenderedMessage);
                    break;
                case "info":
                case "notice":
                    _taskLoggingHelper.LogMessage(MessageImportance.Normal, loggingEvent.RenderedMessage);
                    break;
                case "debug":
                case "fine":
                case "finer":
                case "finest":
                case "trace":
                case "verbose":
                default:
                    _taskLoggingHelper.LogMessage(MessageImportance.Low, loggingEvent.RenderedMessage);
                    break;
            }
        }
    }
}

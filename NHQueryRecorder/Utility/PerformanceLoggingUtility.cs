using System;
using System.Diagnostics;
using log4net;

namespace NHQueryRecorder.Utility
{
    public static class PerformanceLoggingUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="message">Description of the operation. The string &quot; in Nms&quot; will be appended to create the log message</param>
        /// <param name="action"></param>
        public static void RecordTimeTaken(this ILog log, string message, Action action)
        {
            log.RecordTimeTaken(message, () =>
            {
                action();
                return 0;
            }); 
        }

        /// <summary>
        /// Executes a 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="message">Description of the operation. The string &quot; in Nms&quot; will be appended to create the log message</param>
        /// <param name="function"></param>
        public static T RecordTimeTaken<T>(this ILog log, string message, Func<T> function)
        {
            var watch = new Stopwatch();
            watch.Start();
            T result = function();
            watch.Stop();
            log.InfoFormat("{0} in {1}", message, watch.Elapsed);
            return result;
        }
    }
}
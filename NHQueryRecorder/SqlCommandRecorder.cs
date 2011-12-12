using System;
using System.Linq;

namespace NHQueryRecorder
{
    /// <summary>
    /// Records the commands captured from the NHibernate.SQL log and converts the log messages to executable SQL
    /// suitable for creation of test data or similar
    /// </summary>
    public class SqlCommandRecorder : IDisposable
    {
        private readonly LogSpy spy;

        public SqlCommandRecorder()
        {
            spy = new LogSpy();
        }

        /// <summary>
        /// Gets a SqlRecording containing all commands recorded during this instance's lifetime
        /// </summary>
        /// <returns></returns>
        public Recording GetRecording()
        {
            var processor = new LogMessageProcessor();
            var commands = from e in spy.Appender.GetEvents()
                   let loggedSql = e.RenderedMessage
				   let command = processor.ProcessCommand(loggedSql)
                   select command;
            return new Recording(commands);
        }

        public void Dispose()
        {
            spy.Dispose();
        }
    }
}
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
            var processor = new SqlCommandProcessor();
            var commands = from e in spy.Appender.GetEvents()
                   let loggedSql = e.RenderedMessage
                   let executableSql = processor.CreateExecutableSql(e.RenderedMessage)
                   select new RecordedCommand(loggedSql, executableSql);
            return new Recording(commands);
        }

        public void Dispose()
        {
            spy.Dispose();
        }
    }
}
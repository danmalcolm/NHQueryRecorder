using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace NHQueryRecorder
{
	internal class LogSpy : IDisposable
	{
		private const string SqlLoggerName = "NHibernate.SQL";
		private readonly MemoryAppender appender;
		private readonly Logger logger;
		private readonly Level originalLogLevel;

		public LogSpy()
		{
			logger = LogManager.GetLogger(SqlLoggerName).Logger as Logger;
			if (logger == null)
			{
				throw new ApplicationException("Unable to obtain logger" + SqlLoggerName);
			}
			originalLogLevel = logger.Level;
			logger.Level = Level.Debug;

			appender = new MemoryAppender();
			logger.AddAppender(appender);
		}

		public MemoryAppender Appender
		{
			get { return appender; }
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			logger.Level = originalLogLevel;
			logger.RemoveAppender(appender);
		}

		#endregion
	}
}
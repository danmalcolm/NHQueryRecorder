using System;

namespace NHQueryRecorder
{
	/// <summary>
	/// Exception thrown when a sql command string is not in a format expected by logic used to manipulate it
	/// </summary>
	public class UnexpectedSqlFormatException : Exception
	{
		public UnexpectedSqlFormatException(string message, string command) : base(message)
		{
			Command = command;
		}

		public UnexpectedSqlFormatException(string message, string command, Exception innerException) : base(message, innerException)
		{
			Command = command;
		}

		public string Command { get; private set; }

		public override string ToString()
		{
			return string.Format("{0}, Command: {1}", base.ToString(), Command);
		}
	}
}
using System;

namespace NHQueryRecorder
{
    public class RecordedCommand
    {
        public RecordedCommand(string loggedSql, string[] executableCommands)
        {
            LoggedSql = loggedSql;
        	ExecutableCommands = executableCommands;
        }

        /// <summary>
        /// The SQL statements as logged by NHibernate
        /// </summary>
        public string LoggedSql { get; private set; }

		/// <summary>
		/// One or more commands
		/// </summary>
    	public string[] ExecutableCommands { get; private set; }

    	/// <summary>
        /// The SQL statement with parameter values parsed from the originally logged statement and inserted into the 
        /// original statement, suitable for executing against the database
        /// </summary>
		public string ExecutableSql { get { return string.Join(Environment.NewLine, ExecutableCommands); } }

        public override string ToString()
        {
            return string.Format(@"LoggedSql:
{0}
ExecutableSql:
{1}", LoggedSql, ExecutableSql);
        }
    }
}
namespace NHQueryRecorder
{
    public class RecordedCommand
    {
        public RecordedCommand(string loggedSql, string executableSql)
        {
            LoggedSql = loggedSql;
            ExecutableSql = executableSql;
        }

        /// <summary>
        /// The SQL statement as logged by NHibernate
        /// </summary>
        public string LoggedSql { get; private set; }

        /// <summary>
        /// The SQL statement with parameter values parsed from the originally logged statement and inserted into the 
        /// original statement, suitable for executing against the database
        /// </summary>
        public string ExecutableSql { get; private set; }

        public override string ToString()
        {
            return string.Format(@"LoggedSql:
{0}
ExecutableSql:
{1}", LoggedSql, ExecutableSql);
        }
    }
}
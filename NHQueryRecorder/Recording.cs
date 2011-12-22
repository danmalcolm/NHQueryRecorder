using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using NHQueryRecorder.Utility;

namespace NHQueryRecorder
{
    /// <summary>
    /// Contains the sequence of commands that were captured while recording the commands logged by NHibernate
    /// </summary>
    public class Recording
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Recording));

        public Recording(IEnumerable<RecordedCommand> commands)
        {
            Commands = commands.ToList().AsReadOnly();
        }

        public IList<RecordedCommand> Commands { get; private set; }

		/// <param name="batchSize">Specifies how frequently a GO separator will be inserted between the commands</param>
        public string GetAllCommandsExecutableSql(int batchSize = 0)
        {
        	var commands = Commands.Select(x => x.ExecutableSql);
			if(batchSize > 0)
			{
				commands = commands.SelectMany((command, index) =>
        	    {
					if(batchSize > 0 && index > 0 && index % batchSize == 0)
					{
						return new[] { command, "GO" };
					}
					else
					{
						return new[] {command};
					}
        	    });
			}
            return Log.RecordTimeTaken("GetAllCommandsExecutableSql",
                () => string.Join(Environment.NewLine, commands));
        }

		/// <param name="batchSize">Specifies how frequently a GO separator will be inserted between the commands</param>
        public void WriteExecutableSqlToFile(string path, int batchSize = 0)
        {
            string content = GetAllCommandsExecutableSql(batchSize);
            File.WriteAllText(path, content);
        }
    }
}
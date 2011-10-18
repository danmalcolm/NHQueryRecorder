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

        public string GetAllCommandsExecutableSql()
        {
            return Log.RecordTimeTaken("GetAllCommandsExecutableSql",
                () => string.Join(Environment.NewLine, Commands.Select(x => x.ExecutableSql)));
        }

        public void WriteExecutableSqlToFile(string path)
        {
            string content = GetAllCommandsExecutableSql();
            File.WriteAllText(path, content);
        }
    }
}
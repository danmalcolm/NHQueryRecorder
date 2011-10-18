using System.Linq;
using System.Text.RegularExpressions;
using Sprache;

namespace NHQueryRecorder.Parsing
{
    public class CommandSplitter
    {
        /// <summary>
        /// Returns seprate strings for each command if log message contains SQL for a batched command, or the entire
        /// command if it is not a batch command
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public static string[] GetIndividualCommands(string logMessage)
        {
            string pattern = "^Batch\\s+commands:\\s+((command\\s+\\d+:(?<command>.*((\\r\\n)|$))))+";
            var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            var match = regex.Match(logMessage);
            if(!match.Success)
            {
                return new[] {logMessage};
            }
            else
            {
                return RegexUtility.GetCapturedValuesInGroup(match, "command").Select(x => x.Trim()).ToArray();
            }
        }

		public static string[] GetIndividualCommands2(string logMessage)
		{
			var result = BatchedCommandParser.BatchedLog.Parse(logMessage);
			return result.Select(x => string.Concat(x)).ToArray();
		}
    }
}
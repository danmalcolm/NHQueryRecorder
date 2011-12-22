using System.Linq;
using System.Text.RegularExpressions;

namespace NHQueryRecorder
{
    public class CommandSplitter
    {
        /// <summary>
        /// Returns separate strings for each command if log message contains batched commands, or the entire
        /// command if it is not a batch command
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public static string[] GetIndividualCommands(string logMessage)
        {
            const string pattern = @"^
Batch\s+commands:\s+
(?<command>
    # 'command N' heading at start of line
    (?<=\r\n)command\s+\d+: 
    # Sql
    (?<commandsql>
	(?:
         # Any char except quote
		(?:[^'])
        |
        # Quoted sequence of chars or escaped quotes (''). This prevents us from matching 'command N' if it happens to be within quotes
        (?:'(?:[^']|(?:''))*') 
	)+?) # Match as few as possible so we don't match the start of the next command group
)*$";
            var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            var match = regex.Match(logMessage);
            if(!match.Success)
            {
                return new[] {logMessage};
            }
            else
            {
                return RegexUtility.GetCapturedValuesInGroup(match, "commandsql").Select(x => x.Trim()).ToArray();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHQueryRecorder
{
    public class SqlCommandProcessor
    {
        const string CommandRegexPattern = @"^
# The actual insert, update, select etc
(?<command>
	(?:
         # Any char except quote or command terminator
		(?:[^';])
        |
        # Quoted sequence of chars or escaped quotes (''). This prevents us from matching on things expected further on if they are in quotes
        (?:'(?:[^']|(?:''))*')
	)*;?
)
(?<params>
	(?: 
		# Parameter, e.g. @p3 = 1 [Type: Int32 (0)]
		(?<param>@p(?<paramIndex>\d+)\s+=\s*
			(?<paramValue>
				(?:
                    # Any char except quote or [
					(?:(?:[^'\[]))+ 
	        		|
                    # Quoted sequence of chars or escaped quotes (''). This prevents us from matching on next parts we are looking for if they are in quotes
	        		(?:'(?:[^']|(?:''))*') 
				)
			)
			\s\[Type:\s+(?<paramType>\w+)\s+\((?<paramLength>\d+)\)]			
		)
		# Separator between parameters
		(?:,\s*)?
	)*
)";
		private static readonly Regex CommandRegex = new Regex(CommandRegexPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

		const string ParamPlaceholderRegexPattern = "@p(?<index>\\d+)";
		private static readonly Regex ParamPlaceholderRegex = new Regex(ParamPlaceholderRegexPattern);

		/// <summary>
		/// Creates a string containing all commands logged
		/// </summary>
		/// <param name="loggedSql"></param>
		/// <returns></returns>
        public string CreateExecutableSql(string loggedSql)
        {
            var loggedCommands = CommandSplitter.GetIndividualCommands(loggedSql);
        	var commands = loggedCommands.Select(this.ConvertIndividualCommand);
            return string.Join(Environment.NewLine, commands);
        }

        private string ConvertIndividualCommand(string logMessage)
        {
            var match = CommandRegex.Match(logMessage);
            if (!match.Success)
            {
            	throw new UnexpectedSqlFormatException("The format of the SQL log message was not in the expected format. Please review reasons why this might have changed.", logMessage);
            }

            string command = match.Groups["command"].Value;
            var parameterMap = GetParameters(match);

            string commandWithParams = ReplaceParameterPlaceholders(command, parameterMap);
            return commandWithParams;
        }

        private string ReplaceParameterPlaceholders(string command, Dictionary<int, ParameterData> parameterMap)
        {
            return ParamPlaceholderRegex.Replace(command, match =>
            {
                int index = int.Parse(match.Groups["index"].Value);
                if(!parameterMap.ContainsKey(index))
                {
                    throw new UnexpectedSqlFormatException("Could not find parameter data for parameter placeholder " + match.Value, command);
                }
                return parameterMap[index].SqlFormattedValue;
            });
        }

        private Dictionary<int,ParameterData> GetParameters(Match match)
        {
            var indexes = RegexUtility.GetCapturedValuesInGroup(match, "paramIndex");
            var values = RegexUtility.GetCapturedValuesInGroup(match, "paramValue").Select(ConvertValueToNullOrRemoveQuotes).ToArray();
            var types = RegexUtility.GetCapturedValuesInGroup(match, "paramType");
            var lengths = RegexUtility.GetCapturedValuesInGroup(match, "paramLength");
            var parameters = from i in Enumerable.Range(0, indexes.Length)
                             let index = int.Parse(indexes[i])
                             let type = types[i]
                             let length = int.Parse(lengths[i])
                             let value = values[i]
                             select new ParameterData (index, type, length, value);
            return parameters.ToDictionary(x => x.Index);
        }

        private string ConvertValueToNullOrRemoveQuotes(string value)
        {
            // Just strip quotes - makes sure we don't record 'NULL' as null
            if (value == null || value == "NULL")
            {
                // non-enquoted NULL value, means it's legitimate db null
                return null;
            }
            else
            {
                var regex = new Regex("^'|'$");
                return regex.Replace(value, "");
            }
        }

		private class ParameterData
        {
            public ParameterData(int index, string type, int length, string value)
            {
                Index = index;
                Type = type;
                Length = length;
                
                
                Value = value;
            }

            public readonly int Index;
            public readonly string Type;
            public int Length;
            public string Value;
            public string Name { get { return "@p" + Index; } }
            
            public string SqlFormattedValue
            {
                get
                {
                    if(Value == null)
                    {
                        return "NULL";
                    }
                    switch (Type.ToLowerInvariant())
                    {
                        case "datetime":
                            var date = DateTime.ParseExact(Value, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture); //17/10/2011 14:12:40
                            return Enquote(date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss"));
                        case "string":
                        case "guid":
                            return Enquote(Value);
						case "boolean":
                    		return Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ? "1" : "0";
                        default:
                            return Value;
                    }
                }
            }

            private string Enquote(string value)
            {
                return string.Concat("'", value, "'");
            }
        }
    }
}
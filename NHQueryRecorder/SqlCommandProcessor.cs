using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHQueryRecorder
{
    public class SqlCommandProcessor
    {
		const string CommandRegexPattern = "(?<command>[^;]+;?)(?<params>(?<param>@p(?<paramIndex>\\d+)\\s+=\\s+(('(?<paramValue>[^']+)')|(?<paramValue>.+?))\\s+\\[Type:\\s+(?<paramType>\\w+)\\s+\\((?<paramLength>\\w+)\\)\\s*](,\\s+)?)*).*";
		private static readonly Regex CommandRegex = new Regex(CommandRegexPattern, RegexOptions.Compiled);

		const string ParamPlaceholderRegexPattern = "@p(?<index>\\d+)";
		private static readonly Regex ParamPlaceholderRegex = new Regex(ParamPlaceholderRegexPattern);

        public string CreateExecutableSql(string loggedSql)
        {
            var commands = (from message in CommandSplitter.GetIndividualCommands(loggedSql)
                   select ConvertIndividualCommand(message)).ToArray();
            return string.Join(Environment.NewLine, commands);
        }

        private string ConvertIndividualCommand(string logMessage)
        {
            var match = CommandRegex.Match(logMessage);
            if (!match.Success)
            {
            	throw new UnexpectedSqlFormatException("Unrecognised sql format. Please review reasons why this might have changed.", logMessage);
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
            var values = RegexUtility.GetCapturedValuesInGroup(match, "paramValue");
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
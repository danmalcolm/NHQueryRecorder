using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NHQueryRecorder.Utility;

namespace NHQueryRecorder
{
    public class LogMessageProcessor
    {
    	private const string BatchCommandsRegexPattern = @"
^(?:Batch\scommands:)\r\n
(?<commandAndParams>
(?:command\s\d+:)
	# The actual insert, update, select etc
	(?<commandText>
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
					# Unquoted value: everything up to the next [
					[^'\[]+ 
	        		|
			        # Or quoted sequence of chars. Note NH log output for string param values does not escape single quotes.
					# We really can't do any better than match .* and rely on the overall structure of the pattern to identify
					# the closing quote. Note use of Singleline option to allow the dot to match new lines
	        		'.*?' 
				)
				\s\[Type:\s+(?<paramType>\w+)\s+\((?<paramLength>\d+)\)]			
			)
			# Separator between parameters
			(?:,\s*)?
		)*
	)
(?:\r\n)?)+$";

		private static readonly Regex BatchCommandsRegex = new Regex(BatchCommandsRegexPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

    	const string ParamPlaceholderRegexPattern = "@p(?<index>\\d+)";
		private static readonly Regex ParamPlaceholderRegex = new Regex(ParamPlaceholderRegexPattern);

        public RecordedCommand ProcessCommand(string loggedSql)
        {
        	var match = BatchCommandsRegex.Match(loggedSql);
        	var commandAndParamCaptures = match.Groups["commandAndParams"].Captures.ToArray();
        	var commandTextCaptures = match.Groups["commandText"].Captures.ToArray();
			var paramCaptures = match.Groups["param"].Captures.ToArray();
			var paramIndexCaptures = match.Groups["paramIndex"].Captures.ToArray();
			var paramValueCaptures = match.Groups["paramValue"].Captures.ToArray();
			var paramTypeCaptures = match.Groups["paramType"].Captures.ToArray();
			var paramLengthCaptures = match.Groups["paramLength"].Captures.ToArray();

        	var executableCommands = (from commandAndParams in commandAndParamCaptures
        	         let commandText = commandTextCaptures.SingleValueWithin(commandAndParams)
        	         let parameterMap = (from p in paramCaptures.Within(commandAndParams)
        	                             let raw = new
        	                             {
        	                             	Capture = p,
        	                             	Index = Convert.ToInt32(paramIndexCaptures.SingleValueWithin(p)),
        	                             	Value = paramValueCaptures.SingleValueWithin(p),
        	                             	Type = paramTypeCaptures.SingleValueWithin(p),
        	                             	Length = Convert.ToInt32(paramLengthCaptures.SingleValueWithin(p))
        	                             }
        	                             select new ParameterData(raw.Index, raw.Type, raw.Length, raw.Value)).ToDictionary
        	         	(x => x.Index)
        	         let executableCommand = CreateExecutableCommand(commandText, parameterMap)
        	         select executableCommand).ToArray();
			return new RecordedCommand(loggedSql, executableCommands);
        }

		private string CreateExecutableCommand(string commandText, Dictionary<int, ParameterData> parameterMap)
    	{
			string executableSql = ReplaceParameterPlaceholders(commandText, parameterMap);
			return executableSql;
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

		private class ParameterData
        {
            public ParameterData(int index, string type, int length, string value)
            {
                Index = index;
                Type = type;
                Length = length;
				Value = ConvertValueToNullOrRemoveQuotes(value);
            }

            public readonly int Index;
            public readonly string Type;
            public int Length;
            public string Value;
            public string Name { get { return "@p" + Index; } }

			private static string ConvertValueToNullOrRemoveQuotes(string value)
			{
				// Just strip quotes - makes sure we don't record 'NULL' (in quotes) as null
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
                    		string escaped = Value.Replace("'", "''");
							return Enquote(escaped);
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
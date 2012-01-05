using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NHQueryRecorder.Utility;

namespace NHQueryRecorder
{
    internal class LogMessageProcessor
    {
    	public RecordedCommand ProcessCommand(string loggedSql)
        {
        	var match = LogMessageRegexes.BatchedCommandsRegex.Match(loggedSql);
			if(!match.Success)
			{
				match = LogMessageRegexes.NonBatchedCommandRegex.Match(loggedSql);
			}
			if(!match.Success)
			{
				throw new UnexpectedSqlFormatException("Unable to parse logged command", loggedSql);
			}
        	var commandAndParamsCaptures = match.Groups["commandAndParams"].Captures.ToArray();
        	var commandTextCaptures = match.Groups["commandText"].Captures.ToQueue();
			var paramCaptures = match.Groups["param"].Captures.ToQueue();
			var paramIndexCaptures = match.Groups["paramIndex"].Captures.ToQueue();
			var paramValueCaptures = match.Groups["paramValue"].Captures.ToQueue();
			var paramTypeCaptures = match.Groups["paramType"].Captures.ToQueue();
			var paramLengthCaptures = match.Groups["paramLength"].Captures.ToQueue();

			// Imperative loop over captures - doesn't feel right to use declarative linq style when we're mutating queues
    		var executableCommands = new string[commandAndParamsCaptures.Length];
			for (int commandIndex = 0; commandIndex < commandAndParamsCaptures.Length; commandIndex++)
			{
				var commandAndParamsCapture = commandAndParamsCaptures[commandIndex];
				var commandText = commandTextCaptures.DequeueCaptureWithin(commandAndParamsCapture).Value;
				var currentParamCaptures = paramCaptures.DequeueCapturesWithin(commandAndParamsCapture).ToArray();
				var parameterInfos = new List<ParameterData>();
				foreach (var param in currentParamCaptures)
				{
					int index = Convert.ToInt32(paramIndexCaptures.DequeueCaptureWithin(param).Value);
					string type = paramTypeCaptures.DequeueCaptureWithin(param).Value;
					int length = Convert.ToInt32(paramLengthCaptures.DequeueCaptureWithin(param).Value);
					string value = paramValueCaptures.DequeueCaptureWithin(param).Value;
					parameterInfos.Add(new ParameterData(index, type, length, value));
				}

				string executableCommand = CreateExecutableCommand(commandText, parameterInfos.ToDictionary(x => x.Index));
				executableCommands[commandIndex] = executableCommand;
			}
			return new RecordedCommand(loggedSql, executableCommands);
        }

		private string CreateExecutableCommand(string commandText, Dictionary<int, ParameterData> parameterMap)
    	{
			string executableSql = ReplaceParameterPlaceholders(commandText, parameterMap);
			return executableSql;
        }

        private string ReplaceParameterPlaceholders(string command, Dictionary<int, ParameterData> parameterMap)
        {
            return LogMessageRegexes.ParamPlaceholderRegex.Replace(command, match =>
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
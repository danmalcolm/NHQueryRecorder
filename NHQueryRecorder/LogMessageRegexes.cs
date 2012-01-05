using System.Text.RegularExpressions;

namespace NHQueryRecorder
{
	internal static class LogMessageRegexes
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

		private const string NonBatchedCommandRegexPattern = @"
^(?<commandAndParams>
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
)$";
		public static readonly Regex BatchedCommandsRegex = new Regex(BatchCommandsRegexPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
		
		public static readonly Regex NonBatchedCommandRegex = new Regex(NonBatchedCommandRegexPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

		private const string ParamPlaceholderRegexPattern = "@p(?<index>\\d+)";
		
		public static readonly Regex ParamPlaceholderRegex = new Regex(ParamPlaceholderRegexPattern, RegexOptions.Compiled);
	}
}
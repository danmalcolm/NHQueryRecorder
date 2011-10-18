using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace NHQueryRecorder.Parsing
{
	/*
	Batch commands:
command 0:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'A' [Type: String (50)], @p1 = 94eea220-5665-4e3f-9c8d-9f7f00d517a8 [Type: Guid (0)]
command 1:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'B' [Type: String (50)], @p1 = d7390ea0-119d-4d20-b4d3-9f7f00d517bf [Type: Guid (0)]
command 2:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'C' [Type: String (50)], @p1 = c673bab0-123d-48b3-bc29-9f7f00d517c4 [Type: Guid (0)]
command 3:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'D' [Type: String (50)], @p1 = 83c5b098-0e11-4a9c-83a6-9f7f00d517c4 [Type: Guid (0)]
command 4:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'E' [Type: String (50)], @p1 = cabd961f-6ba4-4591-9b5a-9f7f00d517c4 [Type: Guid (0)]
	*/

	public static class BatchedCommandParser
	{
		private static readonly Parser<IEnumerable<char>> BatchIndicator = from s in Parse.String("Batch commands:")
																			   from white in Parse.WhiteSpace.Many()
																			   select "";

		private static readonly Parser<int> CommandIntro = from s in Parse.String("command ")
		                                                                 from d in Parse.Digit.Many()
		                                                                 from c in Parse.Char(':').Once()
		                                                                 select int.Parse(new string(d.ToArray()));


		private static readonly Parser<char> Quote = Parse.Char('\'');

		private static readonly Parser<char> Terminator = Parse.Char(';');

		private static readonly Parser<IEnumerable<char>> EscapedQuotes = from q1 in Quote.Once()
		                                                                  from q2 in Quote.Once()
		                                                                  select q1.Concat(q2);

		private static readonly Parser<string> Literal = Parse.AnyChar.Except(Quote.Or(Terminator)).Many().Text();

		private static readonly Parser<string> QuotedLiteral = (from opening in Quote.Once()
		                                                       from content in Literal.Or(EscapedQuotes)
		                                                       from closing in Quote.Once()
		                                                       select opening.Concat(content).Concat(closing)).Text();

		private static readonly Parser<string> Params = Parse.AnyChar.Except(Parse.String(Environment.NewLine)).Many().Text();

		private static readonly Parser<IEnumerable<string>> CommandSql = Literal.Or(QuotedLiteral).Many();

		private static readonly Parser<string> CommandBody = CommandSql.Select(string.Concat);
		
		private static readonly Parser<string> BatchCommand = from intro in CommandIntro
		                                                      from sql in CommandBody
															  from par in Params
															  from w in Parse.WhiteSpace.AtLeastOnce().Or(Params.End())
		                                                      select sql + par;

		public static readonly Parser<IEnumerable<string>> BatchedLog = from i in BatchIndicator.Once()
		                                                   from command in BatchCommand.Many()
		                                                   select command;

	
	}
}
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NHQueryRecorder.Tests
{
	[TestFixture]
	public class LogMessageProcessorSpecs
	{
		[TestCaseSource("GetTestCases")]
		public string[] when_creating_executable_sql_from_log_message(string original)
		{
			var recordedCommand = new LogMessageProcessor().ProcessCommand(original);
			return recordedCommand.ExecutableCommands;
		}

		public IEnumerable<object> GetTestCases()
		{
			yield return CreateTestCase("Batched Single Insert Statement",
			                            @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Thing 1' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]
",
			                            new[]
			                            {
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Thing 1', 1, '2007-07-01T07:07:07', 1, 1.77, '1ceb77e3-3f2e-42b0-a4ba-9f7f01735739');"
			                            })
				;
			yield return CreateTestCase("Batched Insert Statement With Nulls",
			                            @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = NULL [Type: String (50)], @p1 = NULL [Type: Boolean (0)], @p2 = NULL [Type: DateTime (0)], @p3 = NULL [Type: Int32 (0)], @p4 = NULL [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]
",
			                            new[]
			                            {
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (NULL, NULL, NULL, NULL, NULL, '1ceb77e3-3f2e-42b0-a4ba-9f7f01735739');"
			                            })
				;

			yield return CreateTestCase("Batched Insert Statement With Quoted NULL string",
			                            @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'NULL' [Type: String (50)], @p1 = NULL [Type: Boolean (0)], @p2 = NULL [Type: DateTime (0)], @p3 = NULL [Type: Int32 (0)], @p4 = NULL [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]
",
			                            new[]
			                            {
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('NULL', NULL, NULL, NULL, NULL, '1ceb77e3-3f2e-42b0-a4ba-9f7f01735739');"
			                            })
				;

			yield return CreateTestCase("Batched Single Insert Statement with string param value containing unescaped quote",
			                            @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Mike's Thing 1' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]
",
			                            new[]
			                            {
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Mike''s Thing 1', 1, '2007-07-01T07:07:07', 1, 1.77, '1ceb77e3-3f2e-42b0-a4ba-9f7f01735739');"
			                            })
				;

			yield return CreateTestCase("Batched Single Insert Statement with string containing NHibernate.SQL log",
			                            @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Mike's Thing 1' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]",
			                            new[]
			                            {
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = ''Mike''s Thing 1'' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]', 1, '2007-07-01T07:07:07', 1, 1.77, '1ceb77e3-3f2e-42b0-a4ba-9f7f01735739');"
			                            })
				;

			yield return CreateTestCase("Batched Single Insert Statement with string containing param symbols",
			                            @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Mike @p1 Thing' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]
",
			                            new[]
			                            {
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Mike @p1 Thing', 1, '2007-07-01T07:07:07', 1, 1.77, '1ceb77e3-3f2e-42b0-a4ba-9f7f01735739');"
			                            })
				;


			yield return CreateTestCase("Batched Multiple Insert Statements",
			                            @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Thing 1' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 3be7adb4-fce0-406b-b25d-9f7f017141e4 [Type: Guid (0)]
command 1:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Thing 2' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 02/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 2 [Type: Int32 (0)], @p4 = 2.77 [Type: Decimal (0)], @p5 = 57ff162a-c47f-4697-8d96-9f7f017141f0 [Type: Guid (0)]
command 2:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Thing 3' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 03/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 3 [Type: Int32 (0)], @p4 = 3.77 [Type: Decimal (0)], @p5 = 1aba768d-9ed6-44b4-a4bf-9f7f017141f0 [Type: Guid (0)]
command 3:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Thing 4' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 04/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 4 [Type: Int32 (0)], @p4 = 4.77 [Type: Decimal (0)], @p5 = ed212e86-d4b2-4f95-bd13-9f7f017141f1 [Type: Guid (0)]
command 4:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Thing 5' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 05/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 5 [Type: Int32 (0)], @p4 = 5.77 [Type: Decimal (0)], @p5 = 2f42b217-f805-43a6-86ff-9f7f017141f2 [Type: Guid (0)]",
			                            new[]
			                            {
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Thing 1', 1, '2007-07-01T07:07:07', 1, 1.77, '3be7adb4-fce0-406b-b25d-9f7f017141e4');",
											@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Thing 2', 1, '2007-07-02T07:07:07', 2, 2.77, '57ff162a-c47f-4697-8d96-9f7f017141f0');"
			                            	,
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Thing 3', 1, '2007-07-03T07:07:07', 3, 3.77, '1aba768d-9ed6-44b4-a4bf-9f7f017141f0');"
			                            	,
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Thing 4', 1, '2007-07-04T07:07:07', 4, 4.77, 'ed212e86-d4b2-4f95-bd13-9f7f017141f1');"
			                            	,
			                            	@"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES ('Thing 5', 1, '2007-07-05T07:07:07', 5, 5.77, '2f42b217-f805-43a6-86ff-9f7f017141f2');"
			                            })
				;

			yield return
				CreateTestCase("Batched Single Update Command",
				               @"Batch commands:
command 0:UPDATE Thing SET StringProperty = @p0, BoolProperty = @p1, DateProperty = @p2, IntProperty = @p3, DecimalProperty = @p4 WHERE TestModel1Id = @p5;@p0 = 'Thing 2' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 02/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 2 [Type: Int32 (0)], @p4 = 2.77 [Type: Decimal (0)], @p5 = beef65d5-f4f4-41cc-ae96-9f7f018067d6 [Type: Guid (0)]",
				               new[]
				               {
				               	@"UPDATE Thing SET StringProperty = 'Thing 2', BoolProperty = 1, DateProperty = '2007-07-02T07:07:07', IntProperty = 2, DecimalProperty = 2.77 WHERE TestModel1Id = 'beef65d5-f4f4-41cc-ae96-9f7f018067d6';"
				               });

			yield return CreateTestCase("Select statement with 0 params",
			                            "select thing0_.TestModel1Id as TestModel1_0_, thing0_.StringProperty as StringPr2_0_, thing0_.BoolProperty as BoolProp3_0_, thing0_.DateProperty as DateProp4_0_, thing0_.IntProperty as IntPrope5_0_, thing0_.DecimalProperty as DecimalP6_0_ from Thing thing0_",
			                            new[]
			                            {
			                            	@"select thing0_.TestModel1Id as TestModel1_0_, thing0_.StringProperty as StringPr2_0_, thing0_.BoolProperty as BoolProp3_0_, thing0_.DateProperty as DateProp4_0_, thing0_.IntProperty as IntPrope5_0_, thing0_.DecimalProperty as DecimalP6_0_ from Thing thing0_"
			                            })
				;
		}

		public ITestCaseData CreateTestCase(string name, string original, string[] expected)
		{
			return new TestCaseData(original).SetName(name).Returns(expected);
		}
	}
}
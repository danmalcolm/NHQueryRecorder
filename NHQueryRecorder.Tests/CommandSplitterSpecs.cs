using System.Linq;
using NUnit.Framework;

namespace NHQueryRecorder.Tests.CommandSplitterSpecs
{
    public class when_splitting_batch_command : ContextSpecification
    {
        private string[] commands;

        protected override void because()
        {
            var loggedSql =
                @"Batch commands:
command 0:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'A' [Type: String (50)], @p1 = 94eea220-5665-4e3f-9c8d-9f7f00d517a8 [Type: Guid (0)]
command 1:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'B' [Type: String (50)], @p1 = d7390ea0-119d-4d20-b4d3-9f7f00d517bf [Type: Guid (0)]
command 2:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'C' [Type: String (50)], @p1 = c673bab0-123d-48b3-bc29-9f7f00d517c4 [Type: Guid (0)]
command 3:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'D' [Type: String (50)], @p1 = 83c5b098-0e11-4a9c-83a6-9f7f00d517c4 [Type: Guid (0)]
command 4:INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'E' [Type: String (50)], @p1 = cabd961f-6ba4-4591-9b5a-9f7f00d517c4 [Type: Guid (0)]";
            commands = CommandSplitter.GetIndividualCommands(loggedSql);
        }

        [Test]
        public void should_create_separate_command_for_each_command_in_batch()
        {
            Assert.That(commands.Length, Is.EqualTo(5));
        }

        [Test]
        public void should_trim_start_and_end_of_first_command()
        {
            Assert.AreEqual("INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'A' [Type: String (50)], @p1 = 94eea220-5665-4e3f-9c8d-9f7f00d517a8 [Type: Guid (0)]", commands.First());
        }

        [Test]
        public void should_trim_start_and_end_of_middle_command()
        {
            Assert.AreEqual("INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'C' [Type: String (50)], @p1 = c673bab0-123d-48b3-bc29-9f7f00d517c4 [Type: Guid (0)]", commands.ElementAt(2));
        }

        [Test]
        public void should_trim_start_and_end_of_last_command()
        {
            Assert.AreEqual("INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'E' [Type: String (50)], @p1 = cabd961f-6ba4-4591-9b5a-9f7f00d517c4 [Type: Guid (0)]", commands.Last());
        }
        
    }

     public class when_splitting_batch_command_containing_single_command_with_log_message_as_value_of_string_parameter : ContextSpecification
    {
        private string[] commands;
         private string loggedSql;

         protected override void because()
        {
            loggedSql =
                @"Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = ''Mike''''s Thing 1'' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]";
            commands = CommandSplitter.GetIndividualCommands(loggedSql);
        }

        [Test]
        public void should_create_single_command()
        {
            CollectionAssert.AreEquivalent(new[] { loggedSql}, commands);
        }

        [Test]
        public void should_retain_entire_command()
        {
            string firstStatement = @"INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = 'Batch commands:
command 0:INSERT INTO Thing (StringProperty, BoolProperty, DateProperty, IntProperty, DecimalProperty, TestModel1Id) VALUES (@p0, @p1, @p2, @p3, @p4, @p5);@p0 = ''Mike''''s Thing 1'' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]' [Type: String (50)], @p1 = True [Type: Boolean (0)], @p2 = 01/07/2007 07:07:07 [Type: DateTime (0)], @p3 = 1 [Type: Int32 (0)], @p4 = 1.77 [Type: Decimal (0)], @p5 = 1ceb77e3-3f2e-42b0-a4ba-9f7f01735739 [Type: Guid (0)]";
            Assert.AreEqual(firstStatement, commands.First());
        }

    }

    


    public class when_splitting_non_batch_command : ContextSpecification
    {
        private string[] commands;

        protected override void because()
        {
            var loggedSql = @"INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'A' [Type: String (50)], @p1 = 94eea220-5665-4e3f-9c8d-9f7f00d517a8 [Type: Guid (0)]";
            commands = CommandSplitter.GetIndividualCommands(loggedSql);
        }

        [Test]
        public void should_return_single_command()
        {
            Assert.That(commands.Length, Is.EqualTo(1));
        }

        [Test]
        public void should_return_original_command()
        {
            Assert.AreEqual(@"INSERT INTO Node (NodeName, NodeId) VALUES (@p0, @p1);@p0 = 'A' [Type: String (50)], @p1 = 94eea220-5665-4e3f-9c8d-9f7f00d517a8 [Type: Guid (0)]", commands.Single());
        }
     

    }
}
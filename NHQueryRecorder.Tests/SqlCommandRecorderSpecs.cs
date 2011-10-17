using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHQueryRecorder.Tests.TestModels;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHQueryRecorder.Tests.SqlCommandRecorderSpecs
{
	public abstract class shared_context : PersistenceSpecification
	{
		protected Recording Recording;

		protected void Record(Action<ISession> action)
		{
			using (var recorder = new SqlCommandRecorder())
			{
				InNewSession(action);
				var recording = recorder.GetRecording();
				Console.WriteLine(string.Join(Environment.NewLine, recording.Commands.Select(x => x.ToString())));
				Recording = recording;
			}
		}
	}

	public class when_recording_batched_inserts : shared_context
    {
        protected override void because()
        {
			var things = ThingBuilder.BuildSequence(5).ToList();
			Record(session => things.ForEach(session.SaveOrUpdate));
        }

		[Test]
        public void should_record_single_batch_command()
        {
            Assert.That(Recording.Commands.Count(), Is.EqualTo(1));
        }

		[Test]
		public void recorded_command_should_include_batch_inserts_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			Assert.That(Regex.Matches(command.LoggedSql, "INSERT\\s+(INTO\\s+)?Thing").Count, Is.EqualTo(5));
		}

		[Test]
		public void recorded_command_should_include_executable_version_of_sql_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			string executableSql = new SqlCommandProcessor().CreateExecutableSql(command.LoggedSql);
			Assert.AreEqual(executableSql, command.ExecutableSql);
		}
    }

	public class when_recording_single_insert : shared_context
	{
		protected override void because()
		{
			var thing = ThingBuilder.Build();
			Record(session => session.SaveOrUpdate(thing));
		}

		[Test]
		public void should_record_single_command()
		{
			Assert.That(Recording.Commands.Count(), Is.EqualTo(1));
		}

		[Test]
		public void recorded_command_should_include_insert_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			// very primitive check to ensure sql from log is in place
			Assert.That(Regex.Matches(command.LoggedSql, "INSERT\\s+(INTO\\s+)?Thing").Count, Is.EqualTo(1));
		}

		[Test]
		public void recorded_command_should_include_executable_version_of_sql_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			string executableSql = new SqlCommandProcessor().CreateExecutableSql(command.LoggedSql);
			Assert.AreEqual(executableSql, command.ExecutableSql);
		}
	}

	public class when_recording_single_update : shared_context
	{
		protected override void because()
		{
			var things = ThingBuilder.BuildSequence(2).ToList();
			var original = things.First();
			var other = things.Last();
			InNewSession(session => session.SaveOrUpdate(original));
			original.CopyPropertiesFrom(other);
			Record(session => session.SaveOrUpdate(original));
		}

		[Test]
		public void should_record_single_command()
		{
			Assert.That(Recording.Commands.Count(), Is.EqualTo(1));
		}

		[Test]
		public void recorded_command_should_include_update_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			// very primitive check to ensure sql from log is in place
			Assert.That(Regex.Matches(command.LoggedSql, "UPDATE\\s+Thing").Count, Is.EqualTo(1));
		}

		[Test]
		public void recorded_command_should_include_executable_version_of_sql_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			string executableSql = new SqlCommandProcessor().CreateExecutableSql(command.LoggedSql);
			Assert.AreEqual(executableSql, command.ExecutableSql);
		}
	}

	public class when_recording_select_all : shared_context
	{
		protected override void because()
		{
			var original = ThingBuilder.Build();
			InNewSession(session => session.SaveOrUpdate(original));
			var things = new List<Thing>();
			Record(session => things = session.Query<Thing>().ToList());
		}

		[Test]
		public void should_record_single_command()
		{
			Assert.That(Recording.Commands.Count(), Is.EqualTo(1));
		}

		[Test]
		public void recorded_command_should_include_select_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			// very primitive check to ensure sql from log is in place
			Assert.That(Regex.Matches(command.LoggedSql, "SELECT", RegexOptions.IgnoreCase).Count, Is.EqualTo(1));
		}

		[Test]
		public void recorded_command_should_include_executable_version_of_sql_logged_by_nhibernate()
		{
			var command = Recording.Commands.Single();
			string executableSql = new SqlCommandProcessor().CreateExecutableSql(command.LoggedSql);
			Assert.AreEqual(executableSql, command.ExecutableSql);
		}
	}
}
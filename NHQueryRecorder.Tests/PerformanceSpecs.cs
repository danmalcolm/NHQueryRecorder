using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NHQueryRecorder.Tests.TestModels;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHQueryRecorder.Tests.PerformanceSpecs
{
	public abstract class shared_context : PersistenceSpecification
	{
		protected Recording Recording;

		protected long TimeRecording(Action<ISession> action)
		{
			var watch = new Stopwatch();
			watch.Start();
			using (var recorder = new SqlCommandRecorder())
			{
				InNewSession(action);
				var recording = recorder.GetRecording();
				Console.WriteLine(string.Join(Environment.NewLine, recording.Commands.Select(x => x.ToString())));
				Recording = recording;
			}
			var sql = Recording.GetAllCommandsExecutableSql();
			watch.Stop();
			return watch.ElapsedMilliseconds;
		}
	}

	public class when_recording_1000_inserts : shared_context
    {
		private long timeTaken;
		private Recording recording;

		protected override void because()
        {
			using (var recorder = new SqlCommandRecorder())
			{
				for (var i = 0; i < 10; i++)
				{
					InNewSession(session =>
					{
						var things = ThingBuilder.BuildSequence(100).ToList();
						things.ForEach(session.SaveOrUpdate);
						session.Flush();
					});
				}
				var watch = new Stopwatch();
				watch.Start();
				recording = recorder.GetRecording();
				watch.Stop();
				timeTaken = watch.ElapsedMilliseconds;
        	}
		}

		[Test]
        public void should_take_less_than_2000ms_to_parse_logged_commands()
        {
			Assert.That(timeTaken, Is.LessThan(1000));

        }
    }

	
}
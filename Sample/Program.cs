using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NHQueryRecorder;
using NHQueryRecorder.Tests.TestModels;
using NHibernate;
using log4net.Config;

namespace Sample
{
	class Program
	{
		private static Recording recording;

		protected static void InNewSession(Action<ISession> action)
		{
			var sessionFactory = TestConfigurationSource.SessionFactory;
			using (ISession session = sessionFactory.OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				action(session);
				transaction.Commit();
			}
		}

		static void Main(string[] args)
		{
			BasicConfigurator.Configure();
               
			do
			{
				HaveIt();
				Console.WriteLine("Press any key to run again or q to quit");
			} while (Console.ReadKey().KeyChar != 'q');
		}

		private static void HaveIt()
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
				long timeTaken = watch.ElapsedMilliseconds;
				Console.WriteLine("Time taken to process: {0:N}ms", timeTaken);
			}
		}
	}
}

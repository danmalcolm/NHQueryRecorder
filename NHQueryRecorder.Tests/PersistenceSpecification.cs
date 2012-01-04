using System;
using NHQueryRecorder.Tests.TestModels;
using NHibernate;
using NUnit.Framework;

namespace NHQueryRecorder.Tests
{
	public abstract class PersistenceSpecification : ContextSpecification
	{
		private readonly ISessionFactory sessionFactory = TestConfigurationSource.SessionFactory;

		protected override void before_set_up_context()
		{
			//InNewSession(session => session.Delete());
		}

		protected void InNewSession(Action<ISession> action)
		{
			using (ISession session = sessionFactory.OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				action(session);
				transaction.Commit();
			}
		}

		protected void AssertPropertiesMatch(Thing first, Thing second)
		{
			Assert.That(first.StringProperty, Is.EqualTo(second.StringProperty));
			Assert.That(first.BoolProperty, Is.EqualTo(second.BoolProperty));
			Assert.That(first.DateProperty, Is.EqualTo(second.DateProperty));
			Assert.That(first.IntProperty, Is.EqualTo(second.IntProperty));
			Assert.That(first.DecimalProperty, Is.EqualTo(second.DecimalProperty));
		}
	}
}
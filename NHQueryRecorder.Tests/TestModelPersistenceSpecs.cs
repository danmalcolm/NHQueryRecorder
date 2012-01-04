using System;
using System.Linq;
using NHQueryRecorder.Tests.TestModels;
using NUnit.Framework;

namespace NHQueryRecorder.Tests.TestModelPersistenceSpecs
{
	// checks our mapping / config

	public class when_persisting_and_retrieving_single_instance : PersistenceSpecification
	{
		private Thing original;
		private Thing retrieved;

		protected override void because()
		{
			original = ThingBuilder.Build();
			InNewSession(session => session.SaveOrUpdate(original));
			InNewSession(session => retrieved = session.Get<Thing>(original.Id));
		}

		[Test]
		public void instance_should_be_saved()
		{
			Assert.That(retrieved, Is.Not.Null);
		}

		[Test]
		public void retrieved_properties_should_match_original()
		{
			AssertPropertiesMatch(retrieved, original);
		}
	}

	public class when_persisting_multiple_instances : PersistenceSpecification
	{
		private Thing original;
		private Thing retrieved;

		protected override void because()
		{
			var items = ThingBuilder.BuildSequence(100).ToList();
			InNewSession(session => items.ForEach(session.SaveOrUpdate));
		}

		[Test]
		public void should_save_all_instances()
		{
			
		}
	}

	public class when_updating_and_retrieving_single_instance : PersistenceSpecification
	{
		private Thing original;
		private Thing other;
		private Thing retrieved;

		protected override void because()
		{
			var things = ThingBuilder.BuildSequence(2).ToList();
			original = things.First();
			other = things.Last();
			InNewSession(session => session.SaveOrUpdate(original));
			original.CopyPropertiesFrom(other);
			InNewSession(session => session.SaveOrUpdate(original));
			InNewSession(session => retrieved = session.Get<Thing>(original.Id));
		}

		[Test]
		public void instance_should_be_saved()
		{
			Assert.That(retrieved, Is.Not.Null);
		}

		[Test]
		public void retrieved_properties_should_match_original()
		{
			AssertPropertiesMatch(retrieved, original);
		}
	}
}
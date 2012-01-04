using System;
using System.Collections.Generic;
using System.Linq;

namespace NHQueryRecorder.Tests.TestModels
{
	public static class ThingBuilder
	{
		public static Thing Build()
		{
			return BuildSequence(1).Single();
		}

		public static IEnumerable<Thing> BuildSequence(int count)
		{
			return from number in Enumerable.Range(1, count)
			       select new Thing
			       {
			       	StringProperty = "Thing " + number,
			       	BoolProperty = true,
					DateProperty = new DateTime(2000, 1, 1).AddDays(number),
			       	DecimalProperty = 0.77m + number,
			       	IntProperty = number
			       };
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHQueryRecorder.Utility
{
	internal static class CaptureCollectionUtility
	{
		public static Capture[] ToArray(this CaptureCollection collection)
		{
			var captures = new Capture[collection.Count];
			collection.CopyTo(captures, 0);
			return captures;
		}

		public static Queue<Capture> ToQueue(this CaptureCollection collection)
		{
			return new Queue<Capture>(collection.Cast<Capture>());
		}

		public static IEnumerable<Capture> DequeueCapturesWithin(this Queue<Capture> queue, Capture parent)
		{
			while (queue.Count > 0 && queue.Peek().OccursWithin(parent))
			{
				yield return queue.Dequeue();
			}
		}

		public static Capture DequeueCaptureWithin(this Queue<Capture> queue, Capture parent)
		{
			if (!queue.Peek().OccursWithin(parent))
			{
				throw new InvalidOperationException(
					"The next item in the queue does not occur within the text matched by the parent capture");
			}
			return queue.Dequeue();
		}


		public static bool OccursWithin(this Capture capture, Capture other)
		{
			return capture.Index >= other.Index && capture.Index < other.Index + other.Length;
		}

		public static bool Contains(this Capture capture, Capture child)
		{
			return child.OccursWithin(capture);
		}

		public static Capture SingleWithin(this IEnumerable<Capture> captures, Capture parent)
		{
			return captures.Single(x => x.OccursWithin(parent));
		}

		public static string SingleValueWithin(this IEnumerable<Capture> captures, Capture parent)
		{
			return captures.Single(x => x.OccursWithin(parent)).Value;
		}

		public static IEnumerable<Capture> Within(this IEnumerable<Capture> captures, Capture parent)
		{
			return captures.Where(x => x.OccursWithin(parent));
		}
		


	}
}
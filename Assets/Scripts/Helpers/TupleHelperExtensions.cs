using System;
using UnityEngine;

public static class TupleHelperExtensions
{
	/// <summary>
	/// Checks if one tuple of floats (defining a range) overlaps with another one.
	/// This can probably be made more generic so that it is not just floats, but for now, floats are all I need.
	/// </summary>
	/// <param name="tuple">The base tuple of the comparison.</param>
	/// <param name="other">The tuple being compared.</param>
	/// <param name="tolerance">Extra space for the tuple to be considered overlap (i.e. if there would not be space between two ranges for something else)</param>
	/// <returns>If the tuples overlap.</returns>
	public static bool Overlaps(this Tuple<float, float> tuple, Tuple<float, float> other, float tolerance = 0f)
	{
		// this will be set up so that it works whether item1 or item2 of the tuples are the higher, and factor in the tolerance now instead of the check.
		float tupleMin = Mathf.Min(tuple.Item1, tuple.Item2) - tolerance;
		float tupleMax = Mathf.Max(tuple.Item1, tuple.Item2) + tolerance;
		float otherMin = Mathf.Min(other.Item1, other.Item2) - tolerance;
		float otherMax = Mathf.Max(other.Item1, other.Item2) + tolerance;

		return (tupleMin <= otherMin && tupleMax >= otherMin) || (otherMin <= tupleMin && otherMax >= tupleMin);
	}

	/// <summary>
	/// Takes the tuple and returns the same tuple with any specified values changed.
	/// </summary>
	/// <param name="tuple">The tuple to change.</param>
	/// <param name="nItem1">If not null, the returned tuple will have its Item1 set to this.</param>
	/// <param name="nItem2">If not null, the returned tuple will have its Item2 set to this.</param>
	/// <returns>The modified tuple.</returns>
	public static Tuple<float, float> With(this Tuple<float, float> tuple, float? nItem1 = null, float? nItem2 = null)
	{
		return new Tuple<float, float>(nItem1.HasValue ? nItem1.Value : tuple.Item1, nItem2.HasValue ? nItem2.Value : tuple.Item2);
	}
}

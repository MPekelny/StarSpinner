using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleOverlapResolver
{
	private float _overlapTolerance;

	public PuzzleOverlapResolver(float overlapTolerance)
	{
		_overlapTolerance = overlapTolerance;
	}

	/// <summary>
	/// Makes sure the spinners do not overlap, basically by making sure all spinners are not within a certain range of any other. If it is, it will adjust the rotation of the spinner so it does not overlap any more.
	/// </summary>
	/// <param name="allSpinners">The list of the transforms of all the spinners. Should be at least 2 items in it. This will work if the spinnerToResolve is in this list or not.</param>
	/// <param name="spinnerToResolve">The spinner to be checked for overlap and the one whose rotation will be adjusted if there is any overlap. If null, will ResolveOverlaps for each transform in the parameter list.</param>
	public void ResolveOverlaps(List<Transform> allSpinners, Transform spinnerToResolve = null)
	{
		if (allSpinners == null || allSpinners.Count == 0)
		{
			throw new ArgumentException("Called PuzzleOverlapResolver.ResolveOverlaps with a null list of allSpinners.");
		}

		if (spinnerToResolve == null)
		{
			// If spinnerToResolve is null, call ResolveOverlap on all items in the list.
			foreach (Transform t in allSpinners)
			{
				ResolveOverlaps(allSpinners, t);
			}
		}
		else
		{
			List<Tuple<float, float>> ranges = CreateRangesList(allSpinners, spinnerToResolve);
			MergeOverlappingRanges(ranges);
			CheckAndResolveSpinnerOverlap(spinnerToResolve, ranges);
		}
	}

	private List<Tuple<float, float>> CreateRangesList(List<Transform> allSpinners, Transform spinnerToResolve)
	{
		List<Tuple<float, float>> ranges = new List<Tuple<float, float>>();
		for (int i = 0; i < allSpinners.Count; i++)
		{
			if (allSpinners[i] == spinnerToResolve) continue;

			ranges.Add(new Tuple<float, float>(allSpinners[i].eulerAngles.z - _overlapTolerance, allSpinners[i].eulerAngles.z + _overlapTolerance));
		}

		ranges.Sort((x, y) => x.Item1.CompareTo(y.Item1));

		int count = ranges.Count - 1;
		for (int i = 0; i < count; i++)
		{
			ranges.Add(new Tuple<float, float>(ranges[i].Item1 + 360f, ranges[i].Item2 + 360f));
		}

		return ranges;
	}

	private void MergeOverlappingRanges(List<Tuple<float, float>> ranges)
	{
		int iteration = 0;
		while (iteration < ranges.Count - 1)
		{
			if (ranges[iteration].Overlaps(ranges[iteration + 1], _overlapTolerance))
			{
				ranges[iteration] = ranges[iteration].With(nItem2: ranges[iteration + 1].Item2);
				ranges.RemoveAt(iteration + 1);
			}
			else
			{
				iteration++;
			}
		}
	}

	private void CheckAndResolveSpinnerOverlap(Transform spinnerToResolve, List<Tuple<float, float>> ranges)
	{
		float angle = spinnerToResolve.eulerAngles.z;
		Tuple<float, float> checkingRange = new Tuple<float, float>(angle - _overlapTolerance, angle + _overlapTolerance);
		bool overlapFound = false;
		for (int i = 0; i < ranges.Count && !overlapFound; i++)
		{
			if (checkingRange.Overlaps(ranges[i]))
			{
				overlapFound = true;

				// To resolve the overlap, get the center of the range and adjust the rotation of the spinnerToResolve enough to push it out of the range.
				float center = ranges[i].Item1 + (ranges[i].Item2 - ranges[i].Item1) / 2f;
				float adjustmentToMake = 0f;
				if (angle - center > 0f)
				{
					adjustmentToMake = ranges[i].Item2 + _overlapTolerance - angle;
				}
				else
				{
					adjustmentToMake = ranges[i].Item1 - _overlapTolerance - angle;
				}

				spinnerToResolve.localEulerAngles = new Vector3(0f, 0f, spinnerToResolve.localEulerAngles.z + adjustmentToMake);
			}
		}
	}
}

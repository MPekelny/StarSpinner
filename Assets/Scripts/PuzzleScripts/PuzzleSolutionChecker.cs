using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSolutionChecker
{
	private float _solutionTolerance;

	public PuzzleSolutionChecker(float solutionTolerance)
	{
		_solutionTolerance = solutionTolerance;
	}

	/// <summary>
	/// Checks if the puzzle was solved. A puzzle is considered solved if all spinners' angles are within a certain range of each other (this range defined by solutionTolerance).
	/// </summary>
	/// <param name="spinnerAngles">The list of the current angles of the spinners. We don't care about doing anything with any of the spinners, just if their values, hence why the list is just floats. Should be at least 2 items in it.</param>
	/// <returns>True if the spinners is within the acceptable range, false otherwise.</returns>
	public bool CheckIfSolved(List<float> spinnerAngles)
	{
		if (spinnerAngles == null || spinnerAngles.Count == 0)
		{
			throw new ArgumentException("Called PuzzleSolutionChecker.CheckIfSolved with a null or empty list of angles.");
		}
		else if (spinnerAngles.Count == 1)
		{
			Debug.LogWarning("Called PuzzleSolutionChecker.CheckIfSolved with a list of only 1 item, no need to actually check anything so just returning true, but probably shouldn't have a puzzle with only 1 spinner.");
			return true;
		}

		int originalCount = spinnerAngles.Count;

		AddAdjustedAngles(spinnerAngles);
		return CheckIfAdjustedAnglesInRange(spinnerAngles, originalCount);
	}

	private void AddAdjustedAngles(List<float> spinnerAngles)
	{
		int originalCount = spinnerAngles.Count;
		// Sort the angles from lowest to highest, then add all but the highest to the list increased by 360 (to account for angles being between 0 and 360, but 360 and 0 are effectively the same thing for this purpose).
		spinnerAngles.Sort();
		for (int i = 0; i < originalCount - 1; i++)
		{
			spinnerAngles.Add(spinnerAngles[i] + 360f);
		}
	}

	private bool CheckIfAdjustedAnglesInRange(List<float> spinnerAngles, int originalCount)
	{
		// Check each angle against the one originalCount - 1 in front of it (so that the first item is compared to the original last one, the second is compared to the increased version of the fist, etc). 
		// If the second - the first i less than the tolerance, we are within the range of being solved, so return true immediately.
		for (int i = 0; i < spinnerAngles.Count - originalCount; i++)
		{
			if (spinnerAngles[i + (originalCount - 1)] - spinnerAngles[i] <= _solutionTolerance)
			{
				return true;
			}
		}

		return false;
	}
}

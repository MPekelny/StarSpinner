using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A class that holds the functionality for a puzzzle's spinners, so that code does not have to be directly in the PuzzleScreen.
/// </summary>
public class PuzzleSpinnersHelper
{
	private List<PuzzleSpinner> _puzzleSpinners = new List<PuzzleSpinner>();

	public List<Transform> GetSpinnerTransforms()
	{
		List<Transform> transforms = new List<Transform>();
		transforms.AddRange(_puzzleSpinners.Select(item => item.SpinnerObject.transform));
		return transforms;
	}

	public List<float> GetSpinnerRotations()
	{
		List<float> rotations = new List<float>();
		rotations.AddRange(_puzzleSpinners.Select(item => item.transform.eulerAngles.z));
		return rotations;
	}

	public Transform GetRandomSpinnerTransform()
	{
		if (_puzzleSpinners.Count == 0)
		{
			return null;
		}

		int rNum = UnityEngine.Random.Range(0, _puzzleSpinners.Count);
		return _puzzleSpinners[rNum].transform;
	}

	public float GetSpinnerTransitionTime()
	{
		if (_puzzleSpinners.Count == 0)
		{
			return 0f;
		}

		return _puzzleSpinners[0].TransitionDuration;
	}

	public void CreateSpinners(PuzzleScreen spinnerParent, GameData.SpinnerVisualData[] availableVisualDatas, int numSpinnersToCreate)
	{
		for (int i = 0; i < numSpinnersToCreate; i++)
		{
			PuzzleSpinner spinner = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("PuzzleSpinner", spinnerParent.transform).GetComponent<PuzzleSpinner>();
			spinner.transform.localPosition = Vector3.zero;

			Color c;
			Sprite shape;
			if (i < availableVisualDatas.Length)
			{
				c = availableVisualDatas[i].Color;
				shape = availableVisualDatas[i].Shape;
			}
			else
			{
				// We should not get to the point where there are more spinnrs than visualdatas, but just in case default to a random colour and blank sprite.
				c = HelperMethods.MakeRandomColor();
				shape = null;
			}

			spinner.Init(spinnerParent, c, shape);
			_puzzleSpinners.Add(spinner);
		}
	}

	public void RandomSpinSpinners()
	{
		// I want spinners to be somewhat spread out, so will divide the circle based on the number of spinners there are and give each spinner one of those sections.
		// So for example if there are 4 spinners, one will get spun to a spot between 0 and 90 degrees, one will get a range from 90 to 180, another from 180 to 270, and the last from 270 to 360.
		// This will ensure there isn't the possibiliy of a puzzle starting out almost solved.
		int numSpinners = _puzzleSpinners.Count;
		List<Tuple<float, float>> randomRanges = new List<Tuple<float, float>>(numSpinners);
		for (int i = 0; i < numSpinners; i++)
		{
			float rangeMin = 360f / numSpinners * i;
			float rangeMax = 360f / numSpinners * (i + 1);

			// Add a very slight offset to the min and max, so that each spinner stays in the intended initial section even after any potential overlap resolving.
			randomRanges.Add(new Tuple<float, float>(rangeMin + 0.01f, rangeMax - 0.01f));
		}

		for (int i = 0; i < numSpinners; i++)
		{
			int rNum = UnityEngine.Random.Range(0, randomRanges.Count);
			_puzzleSpinners[i].SpinRandomly(randomRanges[rNum].Item1, randomRanges[rNum].Item2);
			randomRanges.RemoveAt(rNum);
		}
	}

	/// <summary>
	/// Tells each of the spinners to transition to their end state, and then once each of them have, calls the complete callback so the puzzle can move on to the next step of the puzzle complete.
	/// </summary>
	/// <param name="onComplete"></param>
	public void TransitionSpinnersToEndState(Action onComplete)
	{
		int neededSpinCounts = _puzzleSpinners.Count;
		foreach (PuzzleSpinner spinner in _puzzleSpinners)
		{
			spinner.TransitionToEndState(() => 
			{
				neededSpinCounts--;
				if (neededSpinCounts <= 0)
				{
					onComplete?.Invoke();
				}
			});
		}
	}

	public void Cleanup()
	{
		foreach (PuzzleSpinner spinner in _puzzleSpinners)
		{
			spinner.ReturnToPool();
		}

		_puzzleSpinners.Clear();
	}
}

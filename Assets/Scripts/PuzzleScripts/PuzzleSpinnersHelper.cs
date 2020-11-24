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
	private int _hintLockedSpinner = -1;
	public int HintLockedSpinner => _hintLockedSpinner;

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

	public List<float> GetSpinnerObjectRotations()
	{
		List<float> rotations = new List<float>();
		rotations.AddRange(_puzzleSpinners.Select(item => item.GetSpinnerObjectRotation()));
		return rotations;
	}

	public Transform GetRandomSpinnerTransform(out int spinnerIndex)
	{
		if (_puzzleSpinners.Count == 0)
		{
			throw new InvalidOperationException("Attempted to get a random spinner's transform when there are no spinners.");
		}

		int rNum = UnityEngine.Random.Range(0, _puzzleSpinners.Count);
		spinnerIndex = rNum;
		return _puzzleSpinners[rNum].transform;
	}

	public Transform GetSpinnerTransformByIndex(int spinnerIndex)
	{
		if (spinnerIndex >= _puzzleSpinners.Count)
		{
			throw new ArgumentException("Tried to get spinner by index when there are less spinners than the specified index.");
		}

		return _puzzleSpinners[spinnerIndex].transform;
	}

	public float GetSpinnerTransitionTime()
	{
		if (_puzzleSpinners.Count == 0)
		{
			return 0f;
		}

		return _puzzleSpinners[0].TransitionDuration;
	}

	/// <summary>
	/// Creates a set of spinners for the puzzle.
	/// </summary>
	/// <param name="spinnerParent">A reference to the screen that owns the set of spinners.</param>
	/// <param name="availableVisualDatas">The collection of colours and shapes for the spinners to use. If more spinners are made than availables datas, a random colour and no shape will be used for that spinner.</param>
	/// <param name="numSpinnersToCreate">How many spinners to make for the puzzle. Other code ideally be used to make sure this number is not more than the number of available visual datas.</param>
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

	/// <summary>
	/// Causes all spinners to have their rotation values set to random values such that all spinners are mostly spread out.
	/// </summary>
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
	/// Looks for any star objects attached to itself so that the spinner has a reference to them for when something needs to be done with all stars on a particular spinner.
	/// </summary>
	public void HaveSpinnersFindStarChildren()
	{
		foreach (PuzzleSpinner spinner in _puzzleSpinners)
		{
			spinner.FindStarChildren();
		}
	}

	/// <summary>
	/// Sets a random spinner to be switched to its hint locked state, if there is not a hint locked spinner already.
	/// </summary>
	/// <returns>True if a spinner was switched to its hint locked state, false otherwise.</returns>
	public bool HintLockRandomSpinner()
	{
		if (_hintLockedSpinner == -1 && _puzzleSpinners.Count > 0)
		{
			int rNum = UnityEngine.Random.Range(0, _puzzleSpinners.Count);
			_puzzleSpinners[rNum].SetToHintState();
			_hintLockedSpinner = rNum;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Sets a particular spinner to be switched to its hint locked state, if there is not a hint locked spinner already.
	/// </summary>
	/// <param name="spinnerIndex">The index of the spinner to switched to its hint locked state.</param>
	/// <returns>True if the spinner was set to the hint locked state, false otherwise.</returns>
	public bool HintLockSpinner(int spinnerIndex)
	{
		if (_hintLockedSpinner == -1 && spinnerIndex >= 0 && spinnerIndex < _puzzleSpinners.Count)
		{
			_hintLockedSpinner = spinnerIndex;
			_puzzleSpinners[spinnerIndex].SetToHintState();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Sets the rotations of each spinner and its touch object to specified values. If the number of rotation values are not the same as the number of spinners, it will do nothing instead.
	/// </summary>
	/// <param name="mainRotations">The rotation values for the spinners themselves.</param>
	/// <param name="objectRotations">The rotation values for the touchable objects in the spinners.</param>
	/// <returns>True if the rotations were set, false if it does not.</returns>
	public bool SetRotationsForSpinners(List<float> mainRotations, List<float> objectRotations)
	{
		// If the number of either lists is not the same size as the puzzleSpinners, something went wrong with save data (though other code should prevent things from getting here in that case).
		// So, in that case return false so the code calling this can know that happened.
		if (mainRotations.Count != _puzzleSpinners.Count || objectRotations.Count != _puzzleSpinners.Count)
		{
			return false;
		}
		else
		{
			for (int i = 0; i < _puzzleSpinners.Count; i++)
			{
				_puzzleSpinners[i].SetRotations(mainRotations[i], objectRotations[i]);
			}

			return true;
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
		_hintLockedSpinner = -1;
	}
}

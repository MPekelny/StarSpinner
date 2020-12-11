using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleTestScene : MonoBehaviour
{
	// This version just needs to display stars and spinners, not most of the extra stuff the regular puzzle scene does, and this is all for testing anyways, so just going to
	// instantiate/destroy the prefabs instead of going through the object pooler.
	[SerializeField] private Star _starPrefab = null;
	[SerializeField] private PuzzleSpinner _spinnerPrefab = null;

	private List<Star> _stars = new List<Star>();
	private List<PuzzleSpinner> _spinners = new List<PuzzleSpinner>();

	private List<PuzzleData.StarData> _testingStars = new List<PuzzleData.StarData>();
	private int _numSpinnersForTesting = 0;
	private string _puzzleName = "";

	private PuzzleOverlapResolver _overlapResolver = null;
	private PuzzleSolutionChecker _checker = null;

	private void Start()
	{
		_overlapResolver = new PuzzleOverlapResolver(7.5f);
		_checker = new PuzzleSolutionChecker(10f);
	}

	public void Awake()
	{
		
	}

	private void RegenerateTest()
	{
		foreach (Star star in _stars)
		{
			Destroy(star);
		}

		_stars.Clear();

		foreach (PuzzleSpinner spinner in _spinners)
		{
			Destroy(spinner);
		}

		_spinners.Clear();

		int numSpinners = _numSpinnersForTesting;
		List<Tuple<float, float>> randomRanges = new List<Tuple<float, float>>(numSpinners);
		for (int i = 0; i < numSpinners; i++)
		{
			float rangeMin = 360f / numSpinners * i;
			float rangeMax = 360f / numSpinners * (i + 1);

			// Add a very slight offset to the min and max, so that each spinner stays in the intended initial section even after any potential overlap resolving.
			randomRanges.Add(new Tuple<float, float>(rangeMin + 0.01f, rangeMax - 0.01f));
		}

		for (int i = 0; i < _numSpinnersForTesting; i++)
		{
			PuzzleSpinner spinner = Instantiate<PuzzleSpinner>(_spinnerPrefab, transform);
			spinner.Init(CheckOverlap, CheckSolved, Color.white, null);
			int rNum = UnityEngine.Random.Range(0, randomRanges.Count);
			spinner.SpinRandomly(randomRanges[rNum].Item1, randomRanges[rNum].Item2);
			randomRanges.RemoveAt(rNum);
			_spinners.Add(spinner);
		}

		foreach (PuzzleData.StarData data in _testingStars)
		{
			int rNum = UnityEngine.Random.Range(0, _spinners.Count);
			Star star = Instantiate<Star>(_starPrefab, _spinners[rNum].transform);
			star.Init(data.FinalColor, data.Position);
			_stars.Add(star);
		}

		CheckOverlap();
	}

	private void CheckOverlap(PuzzleSpinner spinner = null)
	{
		List<Transform> transforms = new List<Transform>();
		transforms.AddRange(_spinners.Select(item => item.SpinnerObject.transform));
		_overlapResolver.ResolveOverlaps(transforms, spinner != null ? spinner.SpinnerObject.transform : null);
	}

	private void CheckSolved()
	{
		List<float> rotations = new List<float>();
		rotations.AddRange(_spinners.Select(item => item.transform.eulerAngles.z));

		if (_checker.CheckIfSolved(rotations))
		{

		}
	}
}

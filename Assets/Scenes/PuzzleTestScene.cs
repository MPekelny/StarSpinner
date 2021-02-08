using System;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class PuzzleTestScene : MonoBehaviour
{
	public const string DATA_BEING_EDITED_PREFS_KEY = "puzzle_data_being_edited";
	public const string PUZZLE_ID_KEY = "puzzle_id";
	public const string PUZZLE_NAME_KEY = "puzzle_name";
	public const string PUZZLE_NUM_SPINNERS_KEY = "puzzle_num_spinners";
	public const string PUZZLE_STAR_POSITION_X_KEY = "pos_x";
	public const string PUZZLE_STAR_POSITION_Y_KEY = "pos_y";
	public const string PUZZLE_STAR_COLOR_R_KEY = "color_r";
	public const string PUZZLE_STAR_COLOR_G_KEY = "color_g";
	public const string PUZZLE_STAR_COLOR_B_KEY = "color_b";
	public const string PUZZLE_STARS_KEY = "stars";

	// This version just needs to display stars and spinners, not most of the extra stuff the regular puzzle scene does, and this is all for testing anyways, so just going to
	// instantiate/destroy the prefabs instead of going through the object pooler.
	[SerializeField] private Star _starPrefab = null;
	[SerializeField] private PuzzleSpinner _spinnerPrefab = null;
	[SerializeField] private TMPro.TextMeshProUGUI _nameText = null;
	[SerializeField] private GameData.SpinnerVisualData[] _visualDatas = null;

	private List<Star> _stars = new List<Star>();
	private List<PuzzleSpinner> _spinners = new List<PuzzleSpinner>();

	private List<PuzzleData.StarData> _testingStars = new List<PuzzleData.StarData>();
	private int _numSpinnersForTesting = 0;
	private string _puzzleName = "";

	private PuzzleOverlapResolver _overlapResolver = null;
	private PuzzleSolutionChecker _checker = null;

	public void Awake()
	{
		_overlapResolver = new PuzzleOverlapResolver(7.5f);
		_checker = new PuzzleSolutionChecker(10f);
		_nameText.gameObject.SetActive(false);

#if UNITY_EDITOR
		if (EditorPrefs.HasKey(DATA_BEING_EDITED_PREFS_KEY))
		{
			JSONNode node = JSONObject.Parse(EditorPrefs.GetString(DATA_BEING_EDITED_PREFS_KEY));
			_puzzleName = node[PUZZLE_NAME_KEY].Value;
			_numSpinnersForTesting = node[PUZZLE_NUM_SPINNERS_KEY].AsInt;

			JSONArray stars = node[PUZZLE_STARS_KEY].AsArray;
			for (int i = 0; i < stars.Count; i++)
			{
				JSONNode starNode = stars[i];
				Vector2 pos = new Vector2(starNode[PUZZLE_STAR_POSITION_X_KEY], starNode[PUZZLE_STAR_POSITION_Y_KEY]);
				Color color = new Color(starNode[PUZZLE_STAR_COLOR_R_KEY], starNode[PUZZLE_STAR_COLOR_G_KEY], starNode[PUZZLE_STAR_COLOR_B_KEY]);
				_testingStars.Add(new PuzzleData.StarData(pos, color));
			}

			GenerateTest();
		}
#endif
	}

	private void GenerateTest()
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
			spinner.Init(CheckOverlap, CheckSolved, _visualDatas[i].Color, _visualDatas[i].Shape);
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
			int neededCount = _spinners.Count;
			for (int i = 0; i < neededCount; i++)
			{
				_spinners[i].TransitionToEndState(() =>
				{
					neededCount--;
					if (neededCount <= 0)
					{
						for (int j = 0; j < _stars.Count; j++)
						{
							_stars[j].SwitchToEndState();
						}

						_nameText.gameObject.SetActive(true);
						_nameText.text = _puzzleName;
					}
				});
			}
		}
	}
}

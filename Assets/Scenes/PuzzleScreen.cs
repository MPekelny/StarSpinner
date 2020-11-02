using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleScreen : MonoBehaviour
{
	[NonSerialized] public const string SCREEN_NAME = "PuzzleScreen";

	[SerializeField] private GameData _gameData = null;
	[SerializeField] private TMPro.TextMeshProUGUI _puzzleNameText = null;
	[SerializeField] private GameObject _uiParticleObject = null;
	[SerializeField] private GameObject _backButton = null;
	[SerializeField] private GameObject _levelEndButtonsContainer = null;
	[SerializeField] private GameObject _levelEndNextLevelButton = null;

	private List<PuzzleSpinner> _testSpinners = new List<PuzzleSpinner>();
	private List<Star> _stars = new List<Star>();

	private PuzzleData _activePuzzle = null;

	private PuzzleSolutionChecker _solutionChecker = null;
	private PuzzleOverlapResolver _overlapResolver = null;

	public void Awake()
	{
		_solutionChecker = new PuzzleSolutionChecker(_gameData.SolutionTolerance);
		_overlapResolver = new PuzzleOverlapResolver(_gameData.OverlapTolerance);
	}

	public void Start()
	{
		_backButton.SetActive(true);
		_uiParticleObject.SetActive(false);
		_puzzleNameText.gameObject.SetActive(false);
		_puzzleNameText.alpha = 0f;
		_levelEndButtonsContainer.SetActive(false);

		SetupPuzzle();
	}

	private void SetupPuzzle()
	{
		_activePuzzle = GameManager.Instance.GetActivePuzzle();
		if (_activePuzzle == null)
		{
			_activePuzzle = _gameData.PuzzleDatas[0];
		}

		CreateSpinners();
		CreateStars();
		RandomSpinSpinners();
	}

	public void BackButtonPressed()
	{
		Cleanup();
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(LevelSelectScreen.SCREEN_NAME);
	}

	public void NextLevelButtonPressed()
	{
		GameManager.Instance.ScreenTransitionManager.FadeOut(() => 
		{
			GameManager.Instance.SetPuzzleIndexToNext();
			Cleanup();
			SetupPuzzle();
			GameManager.Instance.ScreenTransitionManager.FadeIn();
		});
	}

	/// <summary>
	/// Called by a spinner when it stops being dragged, so that if the position it was dropped on overlaps with another spinner, it will be adjusted so it no longer overlaps.
	/// </summary>
	/// <param name="spinnerToCheck"></param>
	public void CheckSpinnerOverlap(PuzzleSpinner spinnerToCheck)
	{
		List<Transform> transforms = new List<Transform>();
		transforms.AddRange(_testSpinners.Select(item => item.SpinnerObject.transform));
		_overlapResolver.ResolveOverlaps(transforms, spinnerToCheck.SpinnerObject.transform);
	}

	/// <summary>
	/// Called by a spinner when it stops being dragged, after checking its overlap, checks if all the spinners' rotations are near enough to each other to be considered solved.
	/// </summary>
	public void CheckIfSolved()
	{
		List<float> rotations = new List<float>();
		rotations.AddRange(_testSpinners.Select(item => item.transform.eulerAngles.z));

		if (_solutionChecker.CheckIfSolved(rotations))
		{
			PlaySolved();
		}
	}

	/// <summary>
	/// Creates a set of spinners for the puzzle based on the number of spinners in the puzzle data.
	/// </summary>
	private void CreateSpinners()
	{
		for (int i = 0; i < _activePuzzle.NumSpinners; i++)
		{
			PuzzleSpinner spinner = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("PuzzleSpinner", transform).GetComponent<PuzzleSpinner>();
			spinner.transform.localPosition = Vector3.zero;

			Color c;
			if (i < _gameData.SpinnerColors.Length)
			{
				c = _gameData.SpinnerColors[i];
			}
			else
			{
				// Other code should prevent us from making more spinners than there are colors, but just in case,
				// we do not want to get an error for trying to get the eighth color when there are only 7, so in that event, just get a random color.
				c = HelperMethods.MakeRandomColor();
			}

			spinner.Init(this, c);

			_testSpinners.Add(spinner);
		}
	}

	/// <summary>
	/// Takes the star data from the puzzledata and creates a star for each one.
	/// TODO: For now is just instantiating, does need to be pool based in the near future.
	/// </summary>
	private void CreateStars()
	{
		for (int i = 0; i < _activePuzzle.StarDatas.Length; i++)
		{
			int rNum = UnityEngine.Random.Range(0, _testSpinners.Count);
			Star star = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("Star", _testSpinners[rNum].transform).GetComponent<Star>();
			star.Init(_activePuzzle.StarDatas[i].FinalColor, _activePuzzle.StarDatas[i].Position);
			_stars.Add(star);
		}
	}

	/// <summary>
	/// Randomize the rotation for all the spinners. 
	/// To make sure the spinners are mostly spread out, each will be allowed a section of the circle to be where it can be randomized.
	/// </summary>
	private void RandomSpinSpinners()
	{
		int numSpinners = _testSpinners.Count;
		List<Tuple<float, float>> randomRanges = new List<Tuple<float, float>>(numSpinners);
		for (int i = 0; i < numSpinners; i++)
		{
			float rangeMin = 360f / numSpinners * i;
			float rangeMax = 360f / numSpinners * (i + 1);

			randomRanges.Add(new Tuple<float, float>(rangeMin, rangeMax));
		}

		for (int i = 0; i < numSpinners; i++)
		{
			int rNum = UnityEngine.Random.Range(0, randomRanges.Count);
			_testSpinners[i].SpinRandomly(randomRanges[rNum].Item1, randomRanges[rNum].Item2);
			randomRanges.RemoveAt(rNum);
		}

		// Make sure there is no overlap of the spinners after the random spin.
		List<Transform> transforms = new List<Transform>();
		transforms.AddRange(_testSpinners.Select(item => item.SpinnerObject.transform));
		_overlapResolver.ResolveOverlaps(transforms);
	}

	private void PlaySolved()
	{
		_backButton.SetActive(false);
		_uiParticleObject.SetActive(true);

		int neededSpinCount = _testSpinners.Count;
		foreach (PuzzleSpinner t in _testSpinners)
		{
			t.TransitionToEndState(() => 
			{
				neededSpinCount--;
				if (neededSpinCount <= 0)
				{
					for (int i = 0; i < _stars.Count; i++)
					{
						_stars[i].TransitionToEndState();
					}

					_puzzleNameText.text = _activePuzzle.PuzzleName;
					_puzzleNameText.gameObject.SetActive(true);
					_puzzleNameText.DOFade(1f, 0.5f).OnComplete(() =>
					{
						_levelEndButtonsContainer.SetActive(true);
						_levelEndNextLevelButton.SetActive(GameManager.Instance.IsThereANextPuzzle());
					});
				}
			});
		}
	}

	private void Cleanup()
	{
		_backButton.SetActive(true);
		_uiParticleObject.SetActive(false);
		_levelEndButtonsContainer.SetActive(false);
		_puzzleNameText.gameObject.SetActive(false);

		foreach (Star star in _stars)
		{
			star.ReturnToPool();
		}

		foreach (PuzzleSpinner spinner in _testSpinners)
		{
			spinner.ReturnToPool();
		}

		_stars.Clear();
		_testSpinners.Clear();
	}
}

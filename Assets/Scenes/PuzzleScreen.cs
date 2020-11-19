using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleScreen : MonoBehaviour
{
	[NonSerialized] public const string SCREEN_NAME = "PuzzleScreen";

	[SerializeField] private GameData _gameData = null;
	[SerializeField] private TMPro.TextMeshProUGUI _puzzleNameText = null;
	[SerializeField] private GameObject _uiParticleObject = null;
	[SerializeField] private GameObject _backButton = null;
	[SerializeField] private GameObject _hintButton = null;
	[SerializeField] private GameObject _levelEndButtonsContainer = null;
	[SerializeField] private GameObject _levelEndNextLevelButton = null;

	protected List<Star> _stars = new List<Star>();
	protected PuzzleData _activePuzzle = null;

	protected PuzzleSpinnersHelper _puzzleSpinnersHelper = null;
	protected PuzzleSolutionChecker _solutionChecker = null;
	protected PuzzleOverlapResolver _overlapResolver = null;

	public void Awake()
	{
		_puzzleSpinnersHelper = new PuzzleSpinnersHelper();
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

	protected void SetupPuzzle()
	{
		_activePuzzle = GameManager.Instance.GetActivePuzzle();
		if (_activePuzzle == null)
		{
			_activePuzzle = _gameData.PuzzleDatas[0];
		}

		_puzzleSpinnersHelper.CreateSpinners(this, _gameData.SpinnerVisualDatas, _activePuzzle.NumSpinners);
		CreateStars();
		_puzzleSpinnersHelper.RandomSpinSpinners();
		CheckSpinnerOverlap();
	}

	/// <summary>
	/// Ui callback for the back button and the end of level return to level select screen button.
	/// </summary>
	public void BackButtonPressed()
	{
		Cleanup();
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(LevelSelectScreen.SCREEN_NAME);
	}

	/// <summary>
	/// UI callback for the end of level go to next level button.
	/// </summary>
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

	public void HintButtonPressed()
	{
		PopupData data = GameManager.Instance.PopupManager.MakePopupData("Get A Hint?", "Once per puzzle, you can lock one of the spinners into the correct position (In a full mobile version of this game, you'd watch a video to get this hint, but for now, just hint yes to get it). Do you want to get the hint?");
		data.AddButtonData("Yes", () =>
		{
			if (_puzzleSpinnersHelper.HintLockRandomSpinner())
			{
				_hintButton.gameObject.SetActive(false);
				_overlapResolver.ResolveOverlaps(_puzzleSpinnersHelper.GetSpinnerTransforms());
			}
		});

		data.AddButtonData("No");

		GameManager.Instance.PopupManager.AddPopup(data);
	}

	/// <summary>
	/// Called by a spinner when it stops being dragged, so that if the position it was dropped on overlaps with another spinner, it will be adjusted so it no longer overlaps.
	/// </summary>
	/// <param name="spinnerToCheck"></param>
	public void CheckSpinnerOverlap(PuzzleSpinner spinnerToCheck = null)
	{
		List<Transform> spinnerTransforms = _puzzleSpinnersHelper.GetSpinnerTransforms();
		_overlapResolver.ResolveOverlaps(spinnerTransforms, spinnerToCheck != null ? spinnerToCheck.SpinnerObject.transform : null);
	}

	/// <summary>
	/// Called by a spinner when it stops being dragged, after checking its overlap, checks if all the spinners' rotations are near enough to each other to be considered solved.
	/// </summary>
	public void CheckIfSolved()
	{
		List<float> rotations = _puzzleSpinnersHelper.GetSpinnerRotations();
		if (_solutionChecker.CheckIfSolved(rotations))
		{
			PlaySolved();
		}
	}

	/// <summary>
	/// Takes the star data from the puzzledata and creates a star for each one.
	/// </summary>
	private void CreateStars()
	{
		for (int i = 0; i < _activePuzzle.StarDatas.Length; i++)
		{
			Transform spinnerTransform = _puzzleSpinnersHelper.GetRandomSpinnerTransform();
			Star star = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("Star", spinnerTransform).GetComponent<Star>();
			star.Init(_activePuzzle.StarDatas[i].FinalColor, _activePuzzle.StarDatas[i].Position);
			_stars.Add(star);
		}

		_puzzleSpinnersHelper.HaveSpinnersFindStarChildren();
	}

	/// <summary>
	/// For when the puzzle is determined to be completed, tweens/sets everything so that the player knows the level is done and select to go back to the level select screen or go to the next level without having to go back to that screen.
	/// </summary>
	private void PlaySolved()
	{
		_backButton.SetActive(false);
		_uiParticleObject.SetActive(true);
		GameManager.Instance.SaveDataManager.SaveLevelCompleted(_activePuzzle.PuzzleUniqueId);

		_puzzleSpinnersHelper.TransitionSpinnersToEndState(() =>
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
		});
	}

	protected void Cleanup()
	{
		_backButton.SetActive(true);
		_uiParticleObject.SetActive(false);
		_levelEndButtonsContainer.SetActive(false);
		_puzzleNameText.gameObject.SetActive(false);
		_hintButton.gameObject.SetActive(true);

		foreach (Star star in _stars)
		{
			star.ReturnToPool();
		}

		_stars.Clear();
		_puzzleSpinnersHelper.Cleanup();
	}
}

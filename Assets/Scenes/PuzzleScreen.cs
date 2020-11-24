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
	private PuzzleSaveData _saveDataHolder = new PuzzleSaveData();

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

		bool gotValidStaticData = TryLoadStaticSaveDataAndPlaceStars();
		if (!gotValidStaticData)
		{
			CreateStars();
		}

		if (!gotValidStaticData || !TryLoadDynamicSaveDataAndSetRotations())
		{
			_puzzleSpinnersHelper.RandomSpinSpinners();
			CheckSpinnerOverlap();

			UpdateDynamicSaveData();
		}
	}

	/// <summary>
	/// Loads the static save data if it exists and then tries to create the stars for the level based on that data.
	/// </summary>
	/// <returns>True if there is save data and it successfully creates the stars using that data, false if either does not happen.</returns>
	private bool TryLoadStaticSaveDataAndPlaceStars()
	{
		if (GameManager.Instance.SaveDataManager.PuzzleStaticDataExistsForLevel(_activePuzzle.PuzzleUniqueId))
		{
			string staticSaveData = GameManager.Instance.SaveDataManager.GetPuzzleStaticDataForLevel(_activePuzzle.PuzzleUniqueId);
			_saveDataHolder.StaticData.ReadFromJsonString(staticSaveData);
			if (_saveDataHolder.StaticData.NumSpinnersInPuzzle == _activePuzzle.NumSpinners && _saveDataHolder.StaticData.NumStarsInPuzzle == _activePuzzle.StarDatas.Length)
			{
				if (CreateStarsFromSaveData())
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Loads the dynamic save data if it exists and then tries to set the rotations and hint locked state for the spinners from that data.
	/// </summary>
	/// <returns>True if the is save data and it successfully sets the rotations using that data, false if either does not happen.</returns>
	private bool TryLoadDynamicSaveDataAndSetRotations()
	{
		if (GameManager.Instance.SaveDataManager.PuzzleDynamicDataExistsForLevel(_activePuzzle.PuzzleUniqueId))
		{
			string dynamicSaveData = GameManager.Instance.SaveDataManager.GetPuzzleDynamicDataForLevel(_activePuzzle.PuzzleUniqueId);
			_saveDataHolder.DynamicData.ReadFromJsonString(dynamicSaveData);

			if (_saveDataHolder.DynamicData.HintLockedSpinner > -1)
			{
				_puzzleSpinnersHelper.HintLockSpinner(_saveDataHolder.DynamicData.HintLockedSpinner);
				_hintButton.gameObject.SetActive(false);
			}

			if (_puzzleSpinnersHelper.SetRotationsForSpinners(_saveDataHolder.DynamicData.SpinnerRotations, _saveDataHolder.DynamicData.SpinnerTouchObjectRotations))
			{
				return true;
			}
		}

		return false;
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
				UpdateDynamicSaveData();
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
		else
		{
			UpdateDynamicSaveData();
		}
	}

	/// <summary>
	/// Takes the star data from the puzzledata and creates a star for each one.
	/// </summary>
	private void CreateStars()
	{
		for (int i = 0; i < _activePuzzle.StarDatas.Length; i++)
		{
			Transform spinnerTransform = _puzzleSpinnersHelper.GetRandomSpinnerTransform(out int transformIndex);
			_saveDataHolder.StaticData.AddSpinnerForStarItem(transformIndex);
			PlaceStar(spinnerTransform, _activePuzzle.StarDatas[i]);
		}

		_saveDataHolder.StaticData.NumSpinnersInPuzzle = _activePuzzle.NumSpinners;
		_saveDataHolder.StaticData.NumStarsInPuzzle = _activePuzzle.StarDatas.Length;
		GameManager.Instance.SaveDataManager.SavePuzzleStaticDataForLevel(_activePuzzle.PuzzleUniqueId, _saveDataHolder.StaticData.WriteToJsonString());

		_puzzleSpinnersHelper.HaveSpinnersFindStarChildren();
	}

	/// <summary>
	/// Creates the stars for the puzzle and attaches them to a spinner based on save data instead of randomly.
	/// As a back up in case the save data does not have the right number of items, it will fall back to placing them randomly.
	/// </summary>
	/// <returns>True if the stars were placed based on save data and false if it fell back on the random placement.</returns>
	private bool CreateStarsFromSaveData()
	{
		// Just in case.
		if (_activePuzzle.StarDatas.Length != _saveDataHolder.StaticData.SpinnersForStarsList.Count)
		{
			CreateStars();
			return false;
		}

		List<int> spinnerStarList = _saveDataHolder.StaticData.SpinnersForStarsList;
		for (int i = 0; i < _activePuzzle.StarDatas.Length; i++)
		{
			Transform spinnerTransform = _puzzleSpinnersHelper.GetSpinnerTransformByIndex(spinnerStarList[i]);
			PlaceStar(spinnerTransform, _activePuzzle.StarDatas[i]);
		}

		_puzzleSpinnersHelper.HaveSpinnersFindStarChildren();
		return true;
	}

	/// <summary>
	/// Grabs a star from the star pool, attaches it to the specified spinner transform and then sets it up with the matching data.
	/// </summary>
	private void PlaceStar(Transform spinnerToPlaceOn, PuzzleData.StarData dataForStar)
	{
		Star star = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("Star", spinnerToPlaceOn).GetComponent<Star>();
		star.Init(dataForStar.FinalColor, dataForStar.Position);
		_stars.Add(star);
	}

	/// <summary>
	/// For when the puzzle is determined to be completed, tweens/sets everything so that the player knows the level is done and select to go back to the level select screen or go to the next level without having to go back to that screen.
	/// </summary>
	private void PlaySolved()
	{
		_backButton.SetActive(false);
		_uiParticleObject.SetActive(true);
		GameManager.Instance.SaveDataManager.SaveLevelCompleted(_activePuzzle.PuzzleUniqueId);
		GameManager.Instance.SaveDataManager.RemovePuzzleSaveDataForLevel(_activePuzzle.PuzzleUniqueId);

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

	/// <summary>
	/// Updates the values of the Dynamic save data in the save data holder and then writes that data to the save manager.
	/// </summary>
	private void UpdateDynamicSaveData()
	{
		List<float> transRotations = _puzzleSpinnersHelper.GetSpinnerRotations();
		List<float> objectRotations = _puzzleSpinnersHelper.GetSpinnerObjectRotations();

		_saveDataHolder.DynamicData.SpinnerRotations = transRotations;
		_saveDataHolder.DynamicData.SpinnerTouchObjectRotations = objectRotations;
		_saveDataHolder.DynamicData.HintLockedSpinner = _puzzleSpinnersHelper.HintLockedSpinner;

		GameManager.Instance.SaveDataManager.SavePuzzleDynamicDataForLevel(_activePuzzle.PuzzleUniqueId, _saveDataHolder.DynamicData.WriteToJsonString());
	}

	protected void Cleanup()
	{
		_backButton.SetActive(true);
		_uiParticleObject.SetActive(false);
		_levelEndButtonsContainer.SetActive(false);
		_puzzleNameText.gameObject.SetActive(false);
		_hintButton.gameObject.SetActive(true);
		_saveDataHolder.ResetAllData();

		foreach (Star star in _stars)
		{
			star.ReturnToPool();
		}

		_stars.Clear();
		_puzzleSpinnersHelper.Cleanup();
	}
}

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

	[SerializeField] private TMPro.TextMeshProUGUI _getHintButtonText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _levelCompleteText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _returnButtonText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _nextLevelButtonText = null;

	protected List<Star> _stars = new List<Star>();
	protected PuzzleData _activePuzzle = null;

	protected PuzzleSpinnersHelper _puzzleSpinnersHelper = null;
	protected PuzzleSolutionChecker _solutionChecker = null;
	protected PuzzleOverlapResolver _overlapResolver = null;
	private PuzzleSaveData _saveDataHolder = new PuzzleSaveData();

	public void Awake()
	{
		StringManager stringMan = GameManager.Instance.StringManager;
		_getHintButtonText.text = stringMan.GetStringForKey("gameplay_get_hint_button");
		_levelCompleteText.text = stringMan.GetStringForKey("gameplay_level_complete");
		_returnButtonText.text = stringMan.GetStringForKey("gameplay_return");
		_nextLevelButtonText.text = stringMan.GetStringForKey("gameplay_next_level");

		_puzzleSpinnersHelper = new PuzzleSpinnersHelper();
		_solutionChecker = new PuzzleSolutionChecker(_gameData.SolutionTolerance);
		_overlapResolver = new PuzzleOverlapResolver(_gameData.OverlapTolerance);
	}

	public void Start()
	{
		GameManager.Instance.AudioManager.PlayBGM(AudioManager.MENU_BGM, 0.5f);

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

		_puzzleSpinnersHelper.CreateSpinners(this, CheckSpinnerOverlap, CheckIfSolved, _gameData.SpinnerVisualDatas, _activePuzzle.NumSpinners);

		if (GameManager.Instance.SaveDataManager.PuzzleStaticDataExistsForLevel(_activePuzzle.PuzzleUniqueId))
		{
			ReadInStaticSaveData();
		}
		else
		{
			CreateStars();
		}

		if (GameManager.Instance.SaveDataManager.PuzzleDynamicDataExistsForLevel(_activePuzzle.PuzzleUniqueId))
		{
			ReadInDynamicSaveData();
		}
		else
		{
			_puzzleSpinnersHelper.RandomSpinSpinners();
			CheckSpinnerOverlap();

			UpdateDynamicSaveData();
		}
	}

	private void ReadInStaticSaveData()
	{
		string staticSaveData = GameManager.Instance.SaveDataManager.GetPuzzleStaticDataForLevel(_activePuzzle.PuzzleUniqueId);
		_saveDataHolder.StaticData.ReadFromJsonString(staticSaveData);
		bool staticDataShouldBeUpdated = false;
		// If for some reason the saved version is higher than the current version, just make the data fresh.
		if (_saveDataHolder.StaticData.PuzzleVersion > _activePuzzle.CurrentVersionNumber)
		{
			Debug.LogError($"Save data for the puzzle {_activePuzzle.PuzzleUniqueId} exists, but for some reason the version number is greater than the current version. Generating the stars for the puzzle fresh.");
			CreateStars();
		}
		else
		{
			// 1. Check that the save data is updated to match up with the current version.
			if (_saveDataHolder.StaticData.PuzzleVersion < _activePuzzle.CurrentVersionNumber)
			{
				Debug.Log($"Save data for {_activePuzzle.PuzzleUniqueId} is a version older than current, upgrading the save data to match the current version.");
				for (int i = _saveDataHolder.StaticData.PuzzleVersion + 1; i <= _activePuzzle.CurrentVersionNumber; i++)
				{
					_saveDataHolder.StaticData.ApplyPuzzleHistoryData(_activePuzzle.HistoryDatas[i]);
				}

				staticDataShouldBeUpdated = true;
			}

			// 2. Make sure that after any updating, there is the correct number of stars. Generally, this case should not happen, but it needs to be handled in case it does.
			if (!_saveDataHolder.StaticData.EnsureCorrectNumberOfStars(_activePuzzle.StarDatas.Length))
			{
				Debug.LogError($"After upgrade static save data for {_activePuzzle.PuzzleUniqueId}, the number of stars in the data did not match the puzzle data, so an additional adjustment had to be made.");
				staticDataShouldBeUpdated = true;
			}

			// 3. Make sure the stars are distributed among the correct number of spinners.
			if (!_saveDataHolder.StaticData.EnsureStarDistribution(_activePuzzle.NumSpinners))
			{
				Debug.Log($"Number of spinners in static save data for {_activePuzzle.PuzzleUniqueId} was different than the puzzle data, the stars were redistributed to fit the puzzle data.");
				staticDataShouldBeUpdated = true;
			}

			// 4. Create the stars using the save data.
			if (!CreateStarsFromSaveData())
			{
				// The create stars from save data should not fail at this point, but as a final just in case, create them fresh if it does fail for some reason.
				Debug.LogError("Somehow, after all the save data updating, it still failed to create the stars from the save data. So, creating the save data fresh.");
				CreateStars();
			}

			// 5. If marked for updating, update the static save data.
			if (staticDataShouldBeUpdated)
			{
				_saveDataHolder.StaticData.PuzzleVersion = _activePuzzle.CurrentVersionNumber;
				GameManager.Instance.SaveDataManager.SavePuzzleStaticDataForLevel(_activePuzzle.PuzzleUniqueId, _saveDataHolder.StaticData.WriteToJsonString());
			}
		}
	}

	private void ReadInDynamicSaveData()
	{
		string dynamicSaveData = GameManager.Instance.SaveDataManager.GetPuzzleDynamicDataForLevel(_activePuzzle.PuzzleUniqueId);
		_saveDataHolder.DynamicData.ReadFromJsonString(dynamicSaveData);
		bool dynamicDataShouldBeUpdated = false;
		// 6. If the number of spinners in the save data is not the same as the puzzle's current version, adjust it.
		if (!_saveDataHolder.DynamicData.EnsureCorrectNumberOfSpinners(_activePuzzle.NumSpinners))
		{
			Debug.Log($"When loading the dynamic save data for the puzzle {_activePuzzle.PuzzleUniqueId}, the number of saved spinners was different than the puzzle's data, so it was adjusted to match.");
			dynamicDataShouldBeUpdated = true;
		}

		// 7. If there is a hint locked spinner and it is now greater than the number of spinners, randomly rechoose it.
		if (_saveDataHolder.DynamicData.HintLockedSpinner >= _activePuzzle.NumSpinners)
		{
			Debug.Log($"When loading the dynamic save data for the puzzle {_activePuzzle.PuzzleUniqueId}, the hint locked spinner is greater than the number of spinners, so randomly rechoosing it.");
			_saveDataHolder.DynamicData.HintLockedSpinner = UnityEngine.Random.Range(0, _activePuzzle.NumSpinners);
			dynamicDataShouldBeUpdated = true;
		}

		// 8. Set the spinner rotations based on save data.
		if (!_puzzleSpinnersHelper.SetRotationsForSpinners(_saveDataHolder.DynamicData.SpinnerRotations, _saveDataHolder.DynamicData.SpinnerTouchObjectRotations))
		{
			// This should not fail at this point, but just in case it needs to be handled.
			Debug.LogError($"When loading the dynamic save data for the puzzle {_activePuzzle.PuzzleUniqueId}, setting the spinners with the save data failed, so randomly spinning them.");
			_puzzleSpinnersHelper.RandomSpinSpinners();
			dynamicDataShouldBeUpdated = true;
		}

		// 9. Make sure the hint locked spinner is set correctly.
		if (_saveDataHolder.DynamicData.HintLockedSpinner > -1)
		{
			_puzzleSpinnersHelper.HintLockSpinner(_saveDataHolder.DynamicData.HintLockedSpinner);
			_hintButton.gameObject.SetActive(false);
		}

		// 10. If marked for updating, make sure overlaps are resolved and update dynamic save data.
		if (dynamicDataShouldBeUpdated)
		{
			CheckSpinnerOverlap();
			UpdateDynamicSaveData();
		}
	}

	/// <summary>
	/// Ui callback for the back button and the end of level return to level select screen button.
	/// </summary>
	public void BackButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);

		Cleanup();
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(LevelSelectScreen.SCREEN_NAME);
	}

	/// <summary>
	/// UI callback for the end of level go to next level button.
	/// </summary>
	public void NextLevelButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);

		GameManager.Instance.ScreenTransitionManager.FadeOut(() => 
		{
			GameManager.Instance.AudioManager.PlayBGM(AudioManager.GAME_BGM, 0.5f);
			GameManager.Instance.SetPuzzleIndexToNext();
			Cleanup();
			SetupPuzzle();
			GameManager.Instance.ScreenTransitionManager.FadeIn();
		});
	}

	public void HintButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);

		StringManager stringMan = GameManager.Instance.StringManager;
		string titleText = stringMan.GetStringForKey("popup_get_hint_title");
		string bodyText = stringMan.GetStringForKey("popup_get_hint_body");

		PopupData data = GameManager.Instance.PopupManager.MakePopupData(titleText, bodyText);

		string yesText = stringMan.GetStringForKey("popup_get_hint_yes");
		data.AddButtonData(yesText, () =>
		{
			if (_puzzleSpinnersHelper.HintLockRandomSpinner())
			{
				_hintButton.gameObject.SetActive(false);
				_overlapResolver.ResolveOverlaps(_puzzleSpinnersHelper.GetSpinnerTransforms());
				UpdateDynamicSaveData();
			}
		});

		string noText = stringMan.GetStringForKey("popup_get_hint_no");
		data.AddButtonData(noText);

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
		_saveDataHolder.StaticData.PuzzleVersion = _activePuzzle.CurrentVersionNumber;
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
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.PUZZLE_VICTORY_SE);
		GameManager.Instance.AudioManager.PlayBGM(AudioManager.GAME_WIN_BGM);

		_backButton.SetActive(false);
		_hintButton.SetActive(false);
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

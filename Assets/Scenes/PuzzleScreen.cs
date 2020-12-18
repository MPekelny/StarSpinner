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

		_puzzleSpinnersHelper.CreateSpinners(this, CheckSpinnerOverlap, CheckIfSolved, _gameData.SpinnerVisualDatas, _activePuzzle.NumSpinners);

		TryLoadSaveData();
	}

	private void TryLoadSaveData()
	{
		if (GameManager.Instance.SaveDataManager.PuzzleStaticDataExistsForLevel(_activePuzzle.PuzzleUniqueId))
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
				int numSpinnersAtTime = _activePuzzle.HistoryDatas[_saveDataHolder.StaticData.PuzzleVersion].NumSpinners;

				// 1. Check that the save data is updated to match up with the current version.
				if (_saveDataHolder.StaticData.PuzzleVersion < _activePuzzle.CurrentVersionNumber)
				{
					Debug.Log($"Save data for {_activePuzzle.PuzzleUniqueId} is a version older than current, upgrading the save data to match the current version.");
					for (int i = _saveDataHolder.StaticData.PuzzleVersion + 1; i <= _activePuzzle.CurrentVersionNumber; i++)
					{
						for (int j = 0; j < _activePuzzle.HistoryDatas[i].NumStarsAdded; j++)
						{
							int rNum = UnityEngine.Random.Range(0, numSpinnersAtTime);
							_saveDataHolder.StaticData.AddSpinnerForStarItem(rNum);
						}

						for (int j = 0; j < _activePuzzle.HistoryDatas[i].StarsDeleted.Length; j++)
						{
							_saveDataHolder.StaticData.RemoveSpinnerForStarItemAtIndex(_activePuzzle.HistoryDatas[i].StarsDeleted[j]);
						}
					}

					staticDataShouldBeUpdated = true;
				}

				// 2. Make sure that after any updating, there is the correct number of stars. Generally, this case should not happen, but it needs to be handled in case it does.
				if (_saveDataHolder.StaticData.SpinnersForStarsList.Count != _activePuzzle.StarDatas.Length)
				{
					if (_saveDataHolder.StaticData.SpinnersForStarsList.Count > _activePuzzle.StarDatas.Length)
					{
						Debug.LogError($"Something happened when loading save data for puzzle {_activePuzzle.PuzzleUniqueId}, there were more stars in the save data than there are in the puzzle. Removing excess stars.");
						_saveDataHolder.StaticData.RemoveSpinnerForStarItemsStartingAtIndex(_activePuzzle.StarDatas.Length);
					}
					else
					{
						Debug.LogError($"Something happened when loading save data for puzzle {_activePuzzle.PuzzleUniqueId}, there were fewer stars in the save data than there are in the puzzle. Adding additional stars.");
						int numToAdd = _activePuzzle.StarDatas.Length - _saveDataHolder.StaticData.SpinnersForStarsList.Count;
						for (int i = 0; i < numToAdd; i++)
						{
							int rNum = UnityEngine.Random.Range(0, numSpinnersAtTime + 1);
							_saveDataHolder.StaticData.AddSpinnerForStarItem(rNum);
						}
					}

					staticDataShouldBeUpdated = true;
				}

				// 3. Make sure the stars are distributed among the correct number of spinners.
				if (numSpinnersAtTime != _activePuzzle.NumSpinners)
				{
					if (numSpinnersAtTime > _activePuzzle.NumSpinners)
					{
						Debug.Log($"When upgrading puzzle save data for {_activePuzzle.PuzzleUniqueId}, the save data has more spinners than the current version. Redistributing any stars on the extra spinners to the valid ones.");

						// If more, simply take all that are set to spinners equal to or higher than the numSpinners, and set their spinners to a valid one.
						for (int i = 0; i < _saveDataHolder.StaticData.SpinnersForStarsList.Count; i++)
						{
							if (_saveDataHolder.StaticData.SpinnersForStarsList[i] >= _activePuzzle.NumSpinners)
							{
								int rNum = UnityEngine.Random.Range(0, _activePuzzle.NumSpinners);
								_saveDataHolder.StaticData.SpinnersForStarsList[i] = rNum;
							}
						}
					}
					else
					{
						Debug.Log($"When upgrading puzzle save data for {_activePuzzle.PuzzleUniqueId}, the save data has fewer spinners than the current version. Redistributing some stars from current spinners onto the new ones.");
						// If less, take a number of stars from the existing ones and asign them to the higher ones.
						// To make it simpler to reassign, make a list of all indices for each current spinner.
						List<int>[] indicesForSpinners = new List<int>[numSpinnersAtTime];
						for (int i = 0; i < indicesForSpinners.Length; i++)
						{
							indicesForSpinners[i] = new List<int>();
						}

						for (int i = 0; i < _saveDataHolder.StaticData.SpinnersForStarsList.Count; i++)
						{
							indicesForSpinners[_saveDataHolder.StaticData.SpinnersForStarsList[i]].Add(i);
						}

						int numStarsForSpinner = _saveDataHolder.StaticData.SpinnersForStarsList.Count / _activePuzzle.NumSpinners;
						for (int i = numSpinnersAtTime; i < _activePuzzle.NumSpinners; i++)
						{
							for (int j = 0; j < numStarsForSpinner; j++)
							{
								int spinnerToPullFrom = PickSpinnerToPullFromWeighted(indicesForSpinners);
								int star = UnityEngine.Random.Range(0, indicesForSpinners[spinnerToPullFrom].Count);
								_saveDataHolder.StaticData.SpinnersForStarsList[indicesForSpinners[spinnerToPullFrom][star]] = i;
								indicesForSpinners[spinnerToPullFrom].RemoveAt(star);
							}
						}
					}

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
		else
		{
			CreateStars();
		}

		if (GameManager.Instance.SaveDataManager.PuzzleDynamicDataExistsForLevel(_activePuzzle.PuzzleUniqueId))
		{
			string dynamicSaveData = GameManager.Instance.SaveDataManager.GetPuzzleDynamicDataForLevel(_activePuzzle.PuzzleUniqueId);
			_saveDataHolder.DynamicData.ReadFromJsonString(dynamicSaveData);
			bool dynamicDataShouldBeUpdated = false;
			// 6. If the number of spinners in the save data is not the same as the puzzle's current version, adjust it.
			if (_saveDataHolder.DynamicData.SpinnerRotations.Count != _activePuzzle.NumSpinners)
			{
				if (_saveDataHolder.DynamicData.SpinnerRotations.Count > _activePuzzle.NumSpinners)
				{
					Debug.Log($"While loading dynamic save data for the puzzle {_activePuzzle.PuzzleUniqueId}, the data had more spinners than the current version, removing the extras from thge dynamic data.");
					_saveDataHolder.DynamicData.SpinnerRotations.RemoveRange(_activePuzzle.NumSpinners, _saveDataHolder.DynamicData.SpinnerRotations.Count - _activePuzzle.NumSpinners);
					_saveDataHolder.DynamicData.SpinnerTouchObjectRotations.RemoveRange(_activePuzzle.NumSpinners, _saveDataHolder.DynamicData.SpinnerTouchObjectRotations.Count - _activePuzzle.NumSpinners);
				}
				else
				{
					Debug.Log($"While loading dynamic save data for the puzzle {_activePuzzle.PuzzleUniqueId}, the data had fewer spinners than the current version, adding additional equal to the difference.");
					int numToAdd = _activePuzzle.NumSpinners - _saveDataHolder.DynamicData.SpinnerRotations.Count;
					for (int i = 0; i < numToAdd; i++)
					{
						float rRot = UnityEngine.Random.Range(0f, 360f);
						float rObjRot = UnityEngine.Random.Range(0f, 360f);
						_saveDataHolder.DynamicData.SpinnerRotations.Add(rRot);
						_saveDataHolder.DynamicData.SpinnerTouchObjectRotations.Add(rObjRot);
					}
				}

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
		else
		{
			_puzzleSpinnersHelper.RandomSpinSpinners();
			CheckSpinnerOverlap();

			UpdateDynamicSaveData();
		}
	}

	/// <summary>
	/// Used for the save data updating for when the number of spinners has increased from a previous version, it gets which of the spinners to pull a star from, weghted
	/// so it is more likely to pull a star from a spinner with more stars.
	/// </summary>
	private int PickSpinnerToPullFromWeighted(List<int>[] starsSpread)
	{
		int[] weights = new int[starsSpread.Length];
		int weightTotal = 0;
		for (int i = 0; i < starsSpread.Length; i++)
		{
			weightTotal += starsSpread[i].Count;
			weights[i] = weightTotal;
		}

		int rNum = UnityEngine.Random.Range(0, weightTotal);
		for (int i = 0; i < weights.Length; i++)
		{
			if (rNum < weights[i])
			{ 
				return i; 
			}
		}

		// Shouldn't get here, but just in case, need to return something.
		Debug.LogError($"When picking a weighted spinner, did not get one properly, so defaulting to returning 0. Number chosen: {rNum}, Weight total: {weightTotal}");
		return 0;
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

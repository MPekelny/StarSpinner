﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSelectScreen : MonoBehaviour
{
	[NonSerialized] public const string SCREEN_NAME = "LevelSelectScreen";

	[SerializeField] private GameObject _levelSelectGrid = null;
	[SerializeField] private GameObject _previousPageButton = null;
	[SerializeField] private GameObject _nextPageButton = null;
	[SerializeField] private int _numLevelsPerPage = 20;

	private int _currentPageStartNumber = 0;
	private List<LevelSelectButton> _buttons = new List<LevelSelectButton>();
	private PuzzleData[] _allPuzzleDataReference;

	public void Awake()
	{
		_currentPageStartNumber = 0;
		_allPuzzleDataReference = GameManager.Instance.GameDataReference.PuzzleDatas;
		ReloadPage();
	}

	/// <summary>
	/// Called by the individual buttons when they are pressed so the game enters the puzzle for the button that was pressed.
	/// </summary>
	/// <param name="index">The index of the level whose button was pressed.</param>
	public void LevelButtonPressed(int index)
	{
		// If the level was in progress, do not just enter the level, show a popup giving the opportunity to start over or clear the data if the player wants instead of entering where it was left off.
		bool levelInProgress = GameManager.Instance.SaveDataManager.PuzzleStaticDataExistsForLevel(_allPuzzleDataReference[index].PuzzleUniqueId);
		if (levelInProgress)
		{
			PopupData data = GameManager.Instance.PopupManager.MakePopupData("Level In Progress", "You left this puzzle in progress. Do you want to enter the puzzle where you left it, enter it fresh, or just clear your progress on it?");
			data.AddButtonData("Resume", () =>
			{
				EnterLevel(index);
			});

			data.AddButtonData("Restart", () =>
			{
				GameManager.Instance.SaveDataManager.RemovePuzzleSaveDataForLevel(_allPuzzleDataReference[index].PuzzleUniqueId);
				EnterLevel(index);
			});

			data.AddButtonData("Clear", () =>
			{
				GameManager.Instance.SaveDataManager.RemovePuzzleSaveDataForLevel(_allPuzzleDataReference[index].PuzzleUniqueId);
				ReloadPage();
			});

			GameManager.Instance.PopupManager.AddPopup(data);
		}
		else
		{
			EnterLevel(index);
		}
	}

	/// <summary>
	/// Callback for when the clear data button is pressed. Presents a popup that if confirmed clears all save data of the player.
	/// </summary>
	public void ClearSaveDataButtonPressed()
	{
		PopupData data = GameManager.Instance.PopupManager.MakePopupData("Clear All Data?", "Clear all your save data?");
		data.AddButtonData("Yes", () =>
		{
			GameManager.Instance.SaveDataManager.ClearAllSaveData();
			ReloadPage();
		});
		data.AddButtonData("No");

		GameManager.Instance.PopupManager.AddPopup(data);
	}

	/// <summary>
	/// Callback for the previous page button, changes the level buttons shown to be the previous set.
	/// </summary>
	public void PreviousPageButtonPressed()
	{
		// Just in case, don't let the start number go below 0.
		_currentPageStartNumber = Math.Max(0, _currentPageStartNumber - _numLevelsPerPage);
		ReloadPage();
	}

	/// <summary>
	/// Callback for the next page button, changes the level buttons shown to be the next set.
	/// </summary>
	public void NextPageButtonPressed()
	{
		// Just in case, don't do anything (other than making sure the buttons are set correctly) if increasing the start point would go beyond the end of the level list.
		if (_currentPageStartNumber + _numLevelsPerPage > _allPuzzleDataReference.Length)
		{
			SetPageButtonActiveness();
		}
		else
		{
			_currentPageStartNumber += _numLevelsPerPage;
			ReloadPage();
		}
			
	}

	/// <summary>
	/// Clears out the level select buttons and creates a set of buttons for the levels starting at the current start point until there is a full page of buttons or it goes to the last level.
	/// </summary>
	private void ReloadPage()
	{
		CleanUp();

		for (int i = _currentPageStartNumber; i < _currentPageStartNumber + _numLevelsPerPage && i < _allPuzzleDataReference.Length; i++)
		{
			LevelSelectButton.LevelProgressState stateForButton = LevelSelectButton.LevelProgressState.Normal;
			bool isSolved = GameManager.Instance.SaveDataManager.IsLevelCompleted(_allPuzzleDataReference[i].PuzzleUniqueId);
			bool isInProgress = GameManager.Instance.SaveDataManager.PuzzleStaticDataExistsForLevel(_allPuzzleDataReference[i].PuzzleUniqueId);
			if (isInProgress)
			{
				stateForButton = LevelSelectButton.LevelProgressState.InProgress;
			}
			else if (isSolved)
			{
				stateForButton = LevelSelectButton.LevelProgressState.Solved;
			}
			
			LevelSelectButton button = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("LevelSelectButton", _levelSelectGrid.transform).GetComponent<LevelSelectButton>();
			button.Init(this, i, stateForButton);
			_buttons.Add(button);
		}

		SetPageButtonActiveness();
	}

	/// <summary>
	/// Sets the specified level to be the active one and then switches the game to the puzzle screen.
	/// </summary>
	/// <param name="levelIndex">The index of the level data to use for the level.</param>
	private void EnterLevel(int levelIndex)
	{
		CleanUp();

		GameManager.Instance.SetActivePuzzleByIndex(levelIndex);
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(PuzzleScreen.SCREEN_NAME);
	}

	/// <summary>
	/// Turns on or off the previous and next page buttons based on whether there are more levels before or after the current page.
	/// </summary>
	private void SetPageButtonActiveness()
	{
		_previousPageButton.SetActive(_currentPageStartNumber > 0);
		_nextPageButton.SetActive(_currentPageStartNumber + _numLevelsPerPage < _allPuzzleDataReference.Length);
	}

	private void CleanUp()
	{
		foreach (LevelSelectButton button in _buttons)
		{
			button.ReturnToPool();
		}

		_buttons.Clear();
	}
}

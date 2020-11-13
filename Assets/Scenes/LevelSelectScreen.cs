using System;
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

	public void LevelButtonPressed(int index)
	{
		CleanUp();

		GameManager.Instance.SetActivePuzzleByIndex(index);
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(PuzzleScreen.SCREEN_NAME);
	}

	public void ClearSaveDataButtonPressed()
	{
		// One of the next things to add should be a basic popup system and have a popup appear for confirmation before actually clearing the save data.
		GameManager.Instance.SaveDataManager.ClearAllSaveData();
		ReloadPage();
	}

	public void PreviousPageButtonPressed()
	{
		// Just in case, don't let the start number go below 0.
		_currentPageStartNumber = Math.Max(0, _currentPageStartNumber - 20);
		ReloadPage();
	}

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

	private void ReloadPage()
	{
		CleanUp();

		for (int i = _currentPageStartNumber; i < _currentPageStartNumber + _numLevelsPerPage && i < _allPuzzleDataReference.Length; i++)
		{
			bool isSolved = GameManager.Instance.SaveDataManager.IsLevelCompleted(_allPuzzleDataReference[i].PuzzleUniqueId);
			LevelSelectButton button = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("LevelSelectButton", _levelSelectGrid.transform).GetComponent<LevelSelectButton>();
			button.Init(this, i, isSolved);
			_buttons.Add(button);
		}

		SetPageButtonActiveness();
	}

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

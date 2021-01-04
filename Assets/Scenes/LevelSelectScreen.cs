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

	[SerializeField] private TMPro.TextMeshProUGUI _selectLevelText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _clearSaveButtonText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _previousPuzzlesButtonText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _nextPuzzlesButtonText = null;

	private int _currentPageStartNumber = 0;
	private List<LevelSelectButton> _buttons = new List<LevelSelectButton>();
	private PuzzleData[] _allPuzzleDataReference;

	public void Awake()
	{
		StringManager stringMan = GameManager.Instance.StringManager;
		_selectLevelText.text = stringMan.GetStringForKey("select_screen_select_level");
		_clearSaveButtonText.text = stringMan.GetStringForKey("select_screen_clear_save");
		_previousPuzzlesButtonText.text = stringMan.GetStringForKey("select_screen_previous_puzzles_button");
		_nextPuzzlesButtonText.text = stringMan.GetStringForKey("select_screen_next_puzzles_button");

		_currentPageStartNumber = 0;
		_allPuzzleDataReference = GameManager.Instance.GameDataReference.PuzzleDatas;
		ReloadPage();
	}

	public void Start()
	{
		GameManager.Instance.AudioManager.PlayBGM("menu_bgm", 0.5f);
	}

	/// <summary>
	/// Called by the individual buttons when they are pressed so the game enters the puzzle for the button that was pressed.
	/// </summary>
	/// <param name="index">The index of the level whose button was pressed.</param>
	public void LevelButtonPressed(int index)
	{
		GameManager.Instance.AudioManager.PlaySoundEffect("button_pressed");

		// If the level was in progress, do not just enter the level, show a popup giving the opportunity to start over or clear the data if the player wants instead of entering where it was left off.
		bool levelInProgress = GameManager.Instance.SaveDataManager.PuzzleStaticDataExistsForLevel(_allPuzzleDataReference[index].PuzzleUniqueId);
		if (levelInProgress)
		{
			StringManager stringMan = GameManager.Instance.StringManager;

			string titleText = stringMan.GetStringForKey("popup_in_progress_title");
			string bodyText = stringMan.GetStringForKey("popup_in_progress_body");
			PopupData data = GameManager.Instance.PopupManager.MakePopupData(titleText, bodyText);

			string resumeText = stringMan.GetStringForKey("popup_in_progress_resume");
			data.AddButtonData(resumeText, () =>
			{
				EnterLevel(index);
			});

			string restartText = stringMan.GetStringForKey("popup_in_progress_restart");
			data.AddButtonData(restartText, () =>
			{
				GameManager.Instance.SaveDataManager.RemovePuzzleSaveDataForLevel(_allPuzzleDataReference[index].PuzzleUniqueId);
				EnterLevel(index);
			});

			string clearText = stringMan.GetStringForKey("popup_in_progress_clear");
			data.AddButtonData(clearText, () =>
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
		GameManager.Instance.AudioManager.PlaySoundEffect("button_pressed");

		StringManager stringMan = GameManager.Instance.StringManager;
		string titleText = stringMan.GetStringForKey("popup_clear_data_title");
		string bodyText = stringMan.GetStringForKey("popup_clear_data_body");

		PopupData data = GameManager.Instance.PopupManager.MakePopupData(titleText, bodyText);

		string yesText = stringMan.GetStringForKey("popup_clear_data_yes");
		data.AddButtonData(yesText, () =>
		{
			GameManager.Instance.SaveDataManager.ClearAllSaveData();
			ReloadPage();
		});

		string noText = stringMan.GetStringForKey("popup_clear_data_no");
		data.AddButtonData(noText);

		GameManager.Instance.PopupManager.AddPopup(data);
	}

	/// <summary>
	/// Callback for the previous page button, changes the level buttons shown to be the previous set.
	/// </summary>
	public void PreviousPageButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect("button_pressed");

		// Just in case, don't let the start number go below 0.
		_currentPageStartNumber = Math.Max(0, _currentPageStartNumber - _numLevelsPerPage);
		ReloadPage();
	}

	/// <summary>
	/// Callback for the next page button, changes the level buttons shown to be the next set.
	/// </summary>
	public void NextPageButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect("button_pressed");

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
			LevelSelectButton button = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("LevelSelectButton", _levelSelectGrid.transform).GetComponent<LevelSelectButton>();
			button.Init(this, _allPuzzleDataReference[i], i);
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

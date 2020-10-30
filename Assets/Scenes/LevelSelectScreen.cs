using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectScreen : MonoBehaviour
{
	[NonSerialized] public const string SCREEN_NAME = "LevelSelectScreen";

	[SerializeField] private GameData _gameData = null;
	[SerializeField] private GameObject _levelSelectGrid = null;

	private List<LevelSelectButton> _buttons = new List<LevelSelectButton>();

	public void Awake()
	{
		for (int i = 0; i < _gameData.PuzzleDatas.Length; i++)
		{
			LevelSelectButton button = GameManager.Instance.ObjectPoolManager.GetObjectFromPool("LevelSelectButton", _levelSelectGrid.transform).GetComponent<LevelSelectButton>();
			button.Init(this, i);
			_buttons.Add(button);
		}
	}

	public void LevelButtonPressed(int index)
	{
		CleanUp();

		GameManager.Instance.ActivePuzzle = _gameData.PuzzleDatas[index];
		SceneManager.LoadScene(PuzzleScreen.SCREEN_NAME);
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

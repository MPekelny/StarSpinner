﻿using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance = null;
	public static GameManager Instance => _instance;

	[SerializeField] private GameData _gameDataReference = null;
	[SerializeField] private ObjectPoolManager _objectPoolManagerPrefab = null;
	[SerializeField] private ScreenTransitionManager _screenTransitionManagerPrefab = null;

	public GameData GameDataReference => _gameDataReference;
	private int _activePuzzleIndex = 0;

	private ObjectPoolManager _objectPoolManager = null;
	public ObjectPoolManager ObjectPoolManager => _objectPoolManager;

	private ScreenTransitionManager _screenTransitionManager = null;
	public ScreenTransitionManager ScreenTransitionManager => _screenTransitionManager;

	public void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
			CreateManagers();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public PuzzleData GetActivePuzzle()
	{
		if (_activePuzzleIndex >= _gameDataReference.PuzzleDatas.Length)
		{
			throw new InvalidOperationException($"Tried to get the active puzzle when the active puzzle index is beyond the range of the puzzle list. Active puzzle index: {_activePuzzleIndex}");
		}

		return _gameDataReference.PuzzleDatas[_activePuzzleIndex];
	}

	public bool IsThereANextPuzzle()
	{
		int indexChecking = _activePuzzleIndex + 1;
		return indexChecking > 0 && indexChecking < _gameDataReference.PuzzleDatas.Length;
	}

	public void SetPuzzleIndexToNext()
	{
		if (IsThereANextPuzzle())
		{
			SetActivePuzzleByIndex(_activePuzzleIndex + 1);
		}
	}

	public void SetActivePuzzleByIndex(int index)
	{
		if (index < 0 || index >= _gameDataReference.PuzzleDatas.Length)
		{
			throw new InvalidOperationException($"Tried to set the active puzzle to an index beyond the range of the puzzle list. Index attempted: {index}");
		}

		_activePuzzleIndex = index;
	}

	private void CreateManagers()
	{
		_objectPoolManager = Instantiate(_objectPoolManagerPrefab, transform);
		_screenTransitionManager = Instantiate(_screenTransitionManagerPrefab, transform);
	}
}

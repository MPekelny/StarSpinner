using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Star Spinner/Create Puzzle Data")]
public class PuzzleData : ScriptableObject
{
	public const int MIN_NUM_SPINNERS = 2;
	public const int MAX_NUM_SPINNERS = 7;

	[System.Serializable]
	public class HistoryData
	{
		[SerializeField] private int _numSpinners = 4;
		[SerializeField] private int _numStarsAdded = 0;
		[SerializeField] private int[] _starsDeleted = null;

		public int NumSpinners => _numSpinners;
		public int NumStarsAdded => _numStarsAdded;
		public int[] StarsDeleted => _starsDeleted;

		public HistoryData(int numSpinners, int numStarsAdded, List<int> starsDeleted)
		{
			_numSpinners = numSpinners;
			_numStarsAdded = numStarsAdded;

			// Want the list to be from highest to lowest, so sort the list an reverse it before getting the list as array.
			starsDeleted.Sort();
			starsDeleted.Reverse();

			_starsDeleted = starsDeleted.ToArray();
		}
	}

	[System.Serializable]
	public class StarData
	{
		[SerializeField] Vector3 _position = Vector3.zero;
		[SerializeField] Color _finalColor = Color.white;

		public Vector3 Position => _position;
		public Color FinalColor => _finalColor;

		public StarData(Vector3 position, Color finalColor)
		{
			_position = position;
			_finalColor = finalColor;
		}
	}

#if UNITY_EDITOR
	[SerializeField] private string _puzzleImageReferencePath = "";
	public string PuzzleImageReferencePath => _puzzleImageReferencePath;
#endif

	[SerializeField] private int _currentVersionNumber = 0;
	[SerializeField] private string _puzzleUniqueId = "";
	[SerializeField] private string _puzzleName = "";
	[SerializeField] private Sprite _puzzleSolvedSprite = null;
	[SerializeField] [Range(MIN_NUM_SPINNERS, MAX_NUM_SPINNERS)] private int _numSpinners = 4;
	[SerializeField] private StarData[] _starDatas = null;
	[SerializeField] private List<HistoryData> _historyDatas = new List<HistoryData>();

	public int CurrentVersionNumber => _currentVersionNumber;
	public string PuzzleUniqueId => _puzzleUniqueId;
	public string PuzzleName => _puzzleName;
	public Sprite PuzzleSolvedSprite => _puzzleSolvedSprite;
	public int NumSpinners => _numSpinners;
	public StarData[] StarDatas => _starDatas;
	public List<HistoryData> HistoryDatas => _historyDatas;

#if UNITY_EDITOR
	public void SetDataFromEditorTool(string puzzleId, string puzzleName, int numSpinners, Sprite puzzleSolvedTexture, List<EditorWindowStuff.PuzzleEditorStar> editorStarDatas, string puzzleImageReferencePath)
	{
		_puzzleUniqueId = puzzleId;
		_puzzleName = puzzleName;
		_numSpinners = numSpinners;
		_puzzleSolvedSprite = puzzleSolvedTexture;
		_puzzleImageReferencePath = puzzleImageReferencePath;
		_starDatas = new StarData[editorStarDatas.Count];
		for (int i = 0; i < editorStarDatas.Count; i++)
		{
			_starDatas[i] = new StarData(editorStarDatas[i].GamePosition, editorStarDatas[i].EndColour);
		}
	}

	public void AddHistoryData(int numSpinnersForVersion, int numStarsAddedForVersion, List<int> starsDeletedForVersion)
	{
		// If this is the first version, other code should have the numStars be 0 and the starsDeleted list be empty, but just in case, the only data I care about for the first version is the number of spinners,
		// so just ignore the other items.
		if (_historyDatas.Count == 0)
		{
			_historyDatas.Add(new HistoryData(numSpinnersForVersion, 0, new List<int>()));
		}
		else
		{
			// If the number of spinners is the same as the previous version and there were no stars added or removed, do not add anthing to the history data.
			if (_historyDatas[_historyDatas.Count - 1].NumSpinners != numSpinnersForVersion ||
				numStarsAddedForVersion != 0 ||
				starsDeletedForVersion.Count > 0)
			{
				_historyDatas.Add(new HistoryData(numSpinnersForVersion, numStarsAddedForVersion, starsDeletedForVersion));
			}
		}

		_currentVersionNumber = _historyDatas.Count - 1;
	}

	public void RestartHistory(int numSpinnersForVersion)
	{
		_historyDatas.Clear();
		_historyDatas.Add(new HistoryData(numSpinnersForVersion, 0, new List<int>()));
		_currentVersionNumber = _historyDatas.Count - 1;
	}
#endif
}

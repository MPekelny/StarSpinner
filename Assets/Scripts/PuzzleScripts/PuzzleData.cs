using UnityEngine;

[CreateAssetMenu(menuName = "Star Spinner/Create Puzzle Data")]
public class PuzzleData : ScriptableObject
{
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

	[Header("This is used to create the data from a TestPuzzleObject.")]
	[SerializeField] private PuzzleBuilderObject _dataGrabber = null;

	[Header("The actual data for the puzzle.")]
	[SerializeField] private string _puzzleUniqueId = "";
	[SerializeField] private string _puzzleName = "";
	[SerializeField] [Range(2, 7)] private int _numSpinners = 4;
	[SerializeField] private StarData[] _starDatas = null;

	public string PuzzleUniqueId => _puzzleUniqueId;
	public string PuzzleName => _puzzleName;
	public int NumSpinners => _numSpinners;
	public StarData[] StarDatas => _starDatas;

	public void OnValidate()
	{
		if (_dataGrabber != null)
		{
			ProcessPuzzleGameObject();
			_dataGrabber = null;
		}
	}

	private void ProcessPuzzleGameObject()
	{
		if (_dataGrabber == null) return;

		_puzzleName = _dataGrabber.PuzzleName;
		_numSpinners = _dataGrabber.NumSpinners;
		_starDatas = new StarData[_dataGrabber.Stars.Length];
		for (int i = 0; i < _dataGrabber.Stars.Length; i++)
		{
			Vector3 starPos = _dataGrabber.Stars[i].transform.localPosition;
			Color starEndColor = _dataGrabber.Stars[i].color;
			_starDatas[i] = new StarData(starPos, starEndColor);
		}

		if (string.IsNullOrEmpty(_dataGrabber.PuzzleUniqueId))
		{
			// We want there to be at least some sort of unique id, so if there isn't one set in the datagrabber, generate one that is a lowercased name of the puzzle + _ + number of stars.
			_puzzleUniqueId = $"{_puzzleName.ToLower()}_{_starDatas.Length}";
		}
		else
		{
			_puzzleUniqueId = _dataGrabber.PuzzleUniqueId;
		}
	}
}

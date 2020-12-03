using System.Collections.Generic;
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

#if UNITY_EDITOR
	[SerializeField] private string _puzzleImageReferencePath = "";
	public string PuzzleImageReferencePath => _puzzleImageReferencePath;
#endif

	[SerializeField] private string _puzzleUniqueId = "";
	[SerializeField] private string _puzzleName = "";
	[SerializeField] [Range(2, 7)] private int _numSpinners = 4;
	[SerializeField] private StarData[] _starDatas = null;

	public string PuzzleUniqueId => _puzzleUniqueId;
	public string PuzzleName => _puzzleName;
	public int NumSpinners => _numSpinners;
	public StarData[] StarDatas => _starDatas;

#if UNITY_EDITOR
	public void SetDataFromEditorTool(string puzzleId, string puzzleName, int numSpinners, List<EditorWindowStuff.PuzzleEditorStar> editorStarDatas, string puzzleImageReferencePath)
	{
		_puzzleUniqueId = puzzleId;
		_puzzleName = puzzleName;
		_numSpinners = numSpinners;
		_puzzleImageReferencePath = puzzleImageReferencePath;
		_starDatas = new StarData[editorStarDatas.Count];
		for (int i = 0; i < editorStarDatas.Count; i++)
		{
			_starDatas[i] = new StarData(editorStarDatas[i].GamePosition, editorStarDatas[i].EndColour);
		}
	}
#endif
}

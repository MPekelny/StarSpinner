using UnityEngine;

[CreateAssetMenu(menuName = "Star Spinner/Create Game Data")]
public class GameData : ScriptableObject
{
	[System.Serializable]
	public class SpinnerVisualData
	{
		[SerializeField] private Color _color;
		[SerializeField] private Sprite _shape;

		public Color Color => _color;
		public Sprite Shape => _shape;
	}

	[SerializeField] private SpinnerVisualData[] _spinnerVisualDatas = null;
	public SpinnerVisualData[] SpinnerVisualDatas => _spinnerVisualDatas;

	[SerializeField] private PuzzleData[] _puzzleDatas = null;
	public PuzzleData[] PuzzleDatas => _puzzleDatas;

	[SerializeField] private float _overlapTolerance = 5f;
	public float OverlapTolerance => _overlapTolerance;

	[SerializeField] private float _solutionTolerance = 10f;
	public float SolutionTolerance => _solutionTolerance;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Star Spinner/Create Game Data")]
public class GameData : ScriptableObject
{
	[SerializeField] private Color[] _spinnerColors = null;
	public Color[] SpinnerColors => _spinnerColors;

	[SerializeField] private PuzzleData[] _puzzleDatas = null;
	public PuzzleData[] PuzzleDatas => _puzzleDatas;

	[SerializeField] private float _overlapTolerance = 5f;
	public float OverlapTolerance => _overlapTolerance;

	[SerializeField] private float _solutionTolerance = 10f;
	public float SolutionTolerance => _solutionTolerance;
}

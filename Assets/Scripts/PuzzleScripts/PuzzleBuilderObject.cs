using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleBuilderObject : MonoBehaviour
{
	[SerializeField] private Image[] _stars = null;
	[SerializeField] private string _puzzleUniqueId = "";
	[SerializeField] private string _puzzleName = "";
	[SerializeField] [Range(2, 7)] private int _numSpinners = 4;

	public int NumSpinners => _numSpinners;
	public string PuzzleUniqueId => _puzzleUniqueId;
	public string PuzzleName => _puzzleName;
	public Image[] Stars => _stars;
}

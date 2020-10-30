using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleBuilderObject : MonoBehaviour
{
	[SerializeField] private Image[] _stars = null;
	[SerializeField] private string _puzzleName = "";
	[SerializeField] [Range(2, 7)] private int _numSpinners = 4;

	public int NumSpinners => _numSpinners;
	public string PuzzleName => _puzzleName;
	public Image[] Stars => _stars;

	public void SpreadStars(List<PuzzleSpinner> spinners)
	{
		for (int i = 0; i < _stars.Length; i++)
		{
			int rNum = Random.Range(0, spinners.Count);
			_stars[i].transform.SetParent(spinners[rNum].transform, false);
		}
	}
}

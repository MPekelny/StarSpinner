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

	public float ComplexityRating { get; private set; }
	public void GenerateComplexityRating()
    {
		/* From what I have observed, things that make a puzzle more difficult aside from the number of spinners are:
		 *	1. Having more stars (pretty self explanitory).
		 *	2. Having more stars that are closer to the center of the puzzle (the closer to the center a star is, the less it moves when a spinner rotates, making it harder to unscramble a puzzle if it has many stars close to the center).
		 *	3. Having stars being very close to its nearest neighbors (while spinning around spinners really close stars can overlap, making it trickier to figure out the correct position).
		 * 
		 * There may be other factors than these that could be used to determine complexity of a puzzle, but I can't think of any right now.
		 * So, will take the number of stars, average distance of stars from the center and average distance of stars from each other, and then multipply each number by some sort of factor so that
		 * the star number is the strongest factor, the distance from the stars to its closest 4 neighbors is the second biggest factor and the distance from center number is the least strong factor.
		 * 
		 * The various numbers I chose for this are rather arbritrary, but it does sseem to be sorting the puzzles roughly in the way I want them to be, so I am going to go with them.
		 */

		float starFactorMultiplier = 6f;
		float starFactor = (float)StarDatas.Length * starFactorMultiplier;

		float totalDistances = 0f;
		foreach (StarData star in StarDatas)
        {
			totalDistances += star.Position.magnitude;
        }

		float centerSubtractFrom = 400f;
		float centerFactorMultiplier = 2f;
		float centerDistanceFactor = (centerSubtractFrom - (totalDistances / (float)StarDatas.Length)) * centerFactorMultiplier;

		float totalAverageInterDistances = 0f;
		for (int i = 0; i < StarDatas.Length; i++)
        {
			float closestNeighbor = 0f;
			float secondClosestNeighbor = 0f;
			float thirdClosestNeighbor = 0f;
			float fourthClosestNeighbor = 0f;
			for (int j = 0; j < StarDatas.Length; j++)
            {
				if (i == j) continue;

				float starDist = (StarDatas[i].Position - StarDatas[j].Position).magnitude;
				if (starDist > closestNeighbor)
                {
					fourthClosestNeighbor = thirdClosestNeighbor;
					thirdClosestNeighbor = secondClosestNeighbor;
					secondClosestNeighbor = closestNeighbor;
					closestNeighbor = starDist;
                }
				else if (starDist > secondClosestNeighbor)
                {
					fourthClosestNeighbor = thirdClosestNeighbor;
					thirdClosestNeighbor = secondClosestNeighbor;
					secondClosestNeighbor = starDist;
				}
				else if (starDist > thirdClosestNeighbor)
                {
					fourthClosestNeighbor = thirdClosestNeighbor;
					thirdClosestNeighbor = starDist;
				}
				else if (starDist > fourthClosestNeighbor)
                {
					fourthClosestNeighbor = starDist;
                }
            }

			totalAverageInterDistances = (closestNeighbor + secondClosestNeighbor + thirdClosestNeighbor + fourthClosestNeighbor) / 4f;
        }

		// The average distance for this will often be something like between 5 and 15 and I want lower numbers to have higher weight, so for this, the number will be subtracted from a large number (with a multiplier so
		// the size affects things more), and then apply the multiplier to the result.
		float subtractFromNumber = 150f;
		float decreaseMultiplier = 3f;
		float interFactorMultiplier = 4f;
		float interDistanceFactor = (subtractFromNumber - ((totalAverageInterDistances / (float)StarDatas.Length) * decreaseMultiplier)) * interFactorMultiplier;

		ComplexityRating = starFactor + centerDistanceFactor + interDistanceFactor;
    }
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

	public void SetDataFromEditorTool(string puzzleId, string puzzleName, int numSpinners, Sprite puzzleSolvedTexture, List<StarData> editorStarDatas, string puzzleImageReferencePath)
    {
		_puzzleUniqueId = puzzleId;
		_puzzleName = puzzleName;
		_numSpinners = numSpinners;
		_puzzleSolvedSprite = puzzleSolvedTexture;
		_puzzleImageReferencePath = puzzleImageReferencePath;
		_starDatas = editorStarDatas.ToArray();
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

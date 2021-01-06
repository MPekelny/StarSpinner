using SimpleJSON;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PuzzleStaticSaveData
{
	private const string PUZZLE_VERSION_KEY = "puzzle_version";
	private const string NUM_SPINNERS_KEY = "num_spinners";
	private const string SPINNERS_LIST_KEY = "spinners_list";

	public List<int> SpinnersForStarsList { get; private set; } = new List<int>();
	public int NumSpinnersInPuzzle { get; set; } = -1;
	public int PuzzleVersion { get; set; } = 0;

	/// <summary>
	/// Takes the save data and turns it into a json formatted string.
	/// </summary>
	public string WriteToJsonString()
	{
		JSONNode node = new JSONObject();
		node[NUM_SPINNERS_KEY] = NumSpinnersInPuzzle;
		node[PUZZLE_VERSION_KEY] = PuzzleVersion;
		foreach (int item in SpinnersForStarsList)
		{
			node[SPINNERS_LIST_KEY][-1] = item;
		}

		StringBuilder builder = new StringBuilder();
		node.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
		return builder.ToString();
	}

	/// <summary>
	/// Creates the save data from a json formatted string.
	/// </summary>
	public void ReadFromJsonString(string json)
	{
		ResetData();

		JSONNode node = JSONNode.Parse(json);
		NumSpinnersInPuzzle = node[NUM_SPINNERS_KEY];
		PuzzleVersion = node[PUZZLE_VERSION_KEY];

		JSONArray starList = node[SPINNERS_LIST_KEY].AsArray;
		for (int i = 0; i < starList.Count; i++)
		{
			SpinnersForStarsList.Add(starList[i].AsInt);
		}
	}

	/// <summary>
	/// Used as part of upgrading save data between versions of a puzzle, applies the changes that were in a single history data version to the save data.
	/// </summary>
	/// <param name="historyData">The history data to apply.</param>
	public void ApplyPuzzleHistoryData(PuzzleData.HistoryData historyData)
	{
		for (int i = 0; i < historyData.NumStarsAdded; i++)
		{
			int rNum = UnityEngine.Random.Range(0, NumSpinnersInPuzzle);
			SpinnersForStarsList.Add(rNum);
		}

		for (int i = 0; i < historyData.StarsDeleted.Length; i++)
		{
			int starToRemoveIndex = historyData.StarsDeleted[i];
			if (starToRemoveIndex < SpinnersForStarsList.Count)
			{
				SpinnersForStarsList.RemoveAt(starToRemoveIndex);
			}
			else
			{
				Debug.LogError($"When applying puzzle history data to save data, tried to remove star at index {starToRemoveIndex} when there are only {SpinnersForStarsList.Count} stars in the save data.");
			}
		}
	}

	/// <summary>
	/// Given a number of stars the puzzle is meant to have, this method makes sure the number of stars it has in its data matches that number.
	/// </summary>
	/// <param name="correctNumStars">The number of stars there is meant to be.</param>
	/// <returns>True if the data already had the correct number of stars, false otherwise.</returns>
	public bool EnsureCorrectNumberOfStars(int correctNumStars)
	{
		if (correctNumStars < SpinnersForStarsList.Count)
		{
			Debug.LogWarning("The number of stars in the save data was more than the expected number of stars. Removing excess stars to match.");
			SpinnersForStarsList.RemoveRange(correctNumStars, SpinnersForStarsList.Count - correctNumStars);
			return false;
		}
		else if (correctNumStars > SpinnersForStarsList.Count)
		{
			Debug.LogWarning("The number of stars in the save data was less than the expected number of stars. Adding additional stars to match.");
			int numToAdd = correctNumStars - SpinnersForStarsList.Count;
			for (int i = 0; i < numToAdd; i++)
			{
				int rNum = UnityEngine.Random.Range(0, NumSpinnersInPuzzle);
				SpinnersForStarsList.Add(rNum);
			}

			return false;
		}

		return true;
	}

	/// <summary>
	/// Given a number of spinners the puzzle is meant to have, this method redistributes the stars so that there are stars on the correct number of spinners, if the data's number of spinners does not match
	/// the expected amount.
	/// </summary>
	/// <param name="correctNumSpinners">The number of spinners there is meant to be.</param>
	/// <returns>True, if the puzzle already contains the correct number of spinners, false otherwise.</returns>
	public bool EnsureStarDistribution(int correctNumSpinners)
	{
		if (NumSpinnersInPuzzle > correctNumSpinners)
		{
			// In the case of there being more spinners in the data than expected, go through all the stars and any whose assigned spinner is beyond the expected range, randomly choose a valid spinner and reassign it to that.
			Debug.Log("There are more spinners in the save data than expected, redistributing the stars on the extra spinners to the others.");
			for (int i = 0; i < SpinnersForStarsList.Count; i++)
			{
				if (SpinnersForStarsList[i] >= correctNumSpinners)
				{
					int rNum = UnityEngine.Random.Range(0, correctNumSpinners);
					SpinnersForStarsList[i] = rNum;
				}
			}

			NumSpinnersInPuzzle = correctNumSpinners;
			return false;
		}
		else if (NumSpinnersInPuzzle < correctNumSpinners)
		{
			// In the case of there being fewer spinners than expected, randomly pick an amount of stars that are assigned existing spinners and reassign them to the new spinner(s) so that the new spinners have a number of spinners roughly equal to the proportion of stars it should have.
			// i.e. if there are 100 stars and went from 4 to 5 spinners, 20 stars are chosen to be set to the fifth spinner.
			// This picking is weighted so that spinners with more stars are more likely to be picked from (and also prevents trying to pick a star from a spinner with no stars).
			Debug.Log("There are fewer spinners in the save data than expected, redistributing some of the stars from each spinner to the new ones.");
			List<int>[] indicesForSpinners = new List<int>[NumSpinnersInPuzzle];
			for (int i = 0; i < indicesForSpinners.Length; i++)
			{
				indicesForSpinners[i] = new List<int>();
			}

			for (int i = 0; i < SpinnersForStarsList.Count; i++)
			{
				indicesForSpinners[SpinnersForStarsList[i]].Add(i);
			}

			int numStarsForSpinner = SpinnersForStarsList.Count / correctNumSpinners;
			for (int i = NumSpinnersInPuzzle; i < correctNumSpinners; i++)
			{
				for (int j = 0; j < numStarsForSpinner; j++)
				{
					int spinnerToPullFrom = PickSpinnerToPullFromWeighted(indicesForSpinners);
					int star = UnityEngine.Random.Range(0, indicesForSpinners[spinnerToPullFrom].Count);
					SpinnersForStarsList[indicesForSpinners[spinnerToPullFrom][star]] = i;
					indicesForSpinners[spinnerToPullFrom].RemoveAt(star);
				}
			}

			NumSpinnersInPuzzle = correctNumSpinners;
			return false;
		}

		return true;
	}

	public void AddSpinnerForStarItem(int spinnerIndex)
	{
		SpinnersForStarsList.Add(spinnerIndex);
	}

	public void ResetData()
	{
		SpinnersForStarsList.Clear();
		NumSpinnersInPuzzle = -1;
		PuzzleVersion = 0;
	}

	/// <summary>
	/// Used for the save data updating for when the number of spinners has increased from a previous version, it gets which of the spinners to pull a star from, weghted
	/// so it is more likely to pull a star from a spinner with more stars.
	/// </summary>
	private int PickSpinnerToPullFromWeighted(List<int>[] starsSpread)
	{
		int[] weights = new int[starsSpread.Length];
		int weightTotal = 0;
		for (int i = 0; i < starsSpread.Length; i++)
		{
			weightTotal += starsSpread[i].Count;
			weights[i] = weightTotal;
		}

		int rNum = UnityEngine.Random.Range(0, weightTotal);
		for (int i = 0; i < weights.Length; i++)
		{
			if (rNum < weights[i])
			{
				return i;
			}
		}

		// Shouldn't get here, but just in case, need to return something.
		Debug.LogError($"When picking a weighted spinner, did not get one properly, so defaulting to returning 0. Number chosen: {rNum}, Weight total: {weightTotal}");
		return 0;
	}
}

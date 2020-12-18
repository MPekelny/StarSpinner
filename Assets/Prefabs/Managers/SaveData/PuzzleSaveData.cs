using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PuzzleSaveData
{
	public class PuzzleStaticSaveData
	{
		private const string PUZZLE_VERSION_KEY = "puzzle_version";
		private const string NUM_STARS_KEY = "num_stars";
		private const string NUM_SPINNERS_KEY = "num_spinners";
		private const string SPINNERS_LIST_KEY = "spinners_list";

		public List<int> SpinnersForStarsList { get; private set; } = new List<int>();
		public int NumSpinnersInPuzzle { get; set; } = -1;
		public int PuzzleVersion { get; set; } = 0;

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

		public void AddSpinnerForStarItem(int spinnerIndex)
		{
			SpinnersForStarsList.Add(spinnerIndex);
		}

		public void RemoveSpinnerForStarItemAtIndex(int starIndex)
		{
			if (starIndex < SpinnersForStarsList.Count)
			{
				SpinnersForStarsList.RemoveAt(starIndex);
			}
			else
			{
				Debug.LogError("Tried to remove a star from the save data that is out of range of the list.");
			}
		}

		public void RemoveSpinnerForStarItemsStartingAtIndex(int index)
		{
			if (index < SpinnersForStarsList.Count)
			{
				SpinnersForStarsList.RemoveRange(index, SpinnersForStarsList.Count - index);
			}
			else
			{
				Debug.LogError("Tried to remove stars from the save data that is starting out of range of the list.");
			}
		}

		public void ResetData()
		{
			SpinnersForStarsList.Clear();
			NumSpinnersInPuzzle = -1;
			PuzzleVersion = 0;
		}
	}

	public class PuzzleDynamicSaveData
	{
		private const string HINT_SPINNER_KEY = "hint_spinner";
		private const string SPINNER_ROTATIONS_KEY = "spinner_rotations";
		private const string SPINNER_TOUCH_ROTATIONS_KEY = "spinner_touch_rotations";

		public List<float> SpinnerRotations { get; set; } = new List<float>();
		public List<float> SpinnerTouchObjectRotations { get; set; } = new List<float>();
		public int HintLockedSpinner { get; set; } = -1;

		public string WriteToJsonString()
		{
			JSONNode node = new JSONObject();
			node[HINT_SPINNER_KEY] = HintLockedSpinner;
			foreach (float rotation in SpinnerRotations)
			{
				node[SPINNER_ROTATIONS_KEY][-1] = rotation;
			}

			foreach (float rotation in SpinnerTouchObjectRotations)
			{
				node[SPINNER_TOUCH_ROTATIONS_KEY][-1] = rotation;
			}

			StringBuilder builder = new StringBuilder();
			node.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
			return builder.ToString();
		}

		public void ReadFromJsonString(string json)
		{
			ResetData();

			JSONNode node = JSONNode.Parse(json);
			HintLockedSpinner = node[HINT_SPINNER_KEY].AsInt;
			JSONArray rotationsArray = node[SPINNER_ROTATIONS_KEY].AsArray;
			for (int i = 0; i < rotationsArray.Count; i++)
			{
				SpinnerRotations.Add(rotationsArray[i].AsFloat);
			}

			JSONArray objectRotationsArray = node[SPINNER_TOUCH_ROTATIONS_KEY].AsArray;
			for (int i = 0; i < objectRotationsArray.Count; i++)
			{
				SpinnerTouchObjectRotations.Add(objectRotationsArray[i].AsFloat);
			}
		}

		public void ResetData()
		{
			SpinnerRotations.Clear();
			SpinnerTouchObjectRotations.Clear();
			HintLockedSpinner = -1;
		}
	}

	// Save data for puzzles is divided into two parts, static and dynamic.
	// Static data is the data that only needs to be set once per level, the list of spinners each star got attached to as well as how many spinners and stars were in the puzzle at the time the level was entered so that
	//    if the puzzle was changed since it was saved, it can regenerate the level rather than trying to use data that will not work correctly. This way that data does not need to be repeatedly be resaved when other data changes.
	// Dynamic data is the data that needs to be changed relatively frequently, namely the rotation values of the spinners and which spinner was hint locked.
	public PuzzleStaticSaveData StaticData { get; private set; } = new PuzzleStaticSaveData();
	public PuzzleDynamicSaveData DynamicData { get; private set; } = new PuzzleDynamicSaveData();

	public void ResetAllData()
	{
		StaticData.ResetData();
		DynamicData.ResetData();
	}
}

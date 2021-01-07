using SimpleJSON;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// TODO: the game is using dynamic and static data as strings. The data is now going to be stored as actual datas now with them just being converted to strings for saving into the playerprefs (and later save files).
// To facilitate switching things over, the methods other code uses to grab the data will still convert to strings and return them. But, at some point it needs to be switched to using the data directly.
public class SaveDataManager : MonoBehaviour
{
	private const string SOLVED_PUZZLES_KEY = "solved_puzzles";
	private const string STATIC_DATA_KEY = "static_datas";
	private const string DYNAMIC_DATA_KEY = "dynamic_datas";
	private const string PUZZLE_ID_KEY = "puzzle_id";
	private const string DATA_KEY = "data";

	PuzzleCompletionSaveData _completionData = new PuzzleCompletionSaveData();
	Dictionary<string, PuzzleDynamicSaveData> _dynamicSaveDatas = new Dictionary<string, PuzzleDynamicSaveData>();
	Dictionary<string, PuzzleStaticSaveData> _staticSaveDatas = new Dictionary<string, PuzzleStaticSaveData>();

	public bool IsLevelCompleted(string levelId) => _completionData.GetLevelCompleted(levelId);
	public bool PuzzleStaticDataExistsForLevel(string levelId) => _staticSaveDatas.ContainsKey(levelId);
	public bool PuzzleDynamicDataExistsForLevel(string levelId) => _dynamicSaveDatas.ContainsKey(levelId);

	private void Start()
	{
		ReadInCompletionData();
		ReadInStaticData();
		ReadInDynamicData();
	}

	/// <summary>
	/// Saves that a given level has been completed.
	/// </summary>
	/// <param name="levelId">The id of the level that was completed.</param>
	public void SaveLevelCompleted(string levelId)
	{
		bool levelWasAdded = _completionData.SetLevelCompleted(levelId);
		if (levelWasAdded)
		{
			WriteOutCompletionData();
		}
	}

	/// <summary>
	/// Gets the static save data for a level, if in the save data.
	/// </summary>
	/// <param name="levelId">The id of the level to get the data for.</param>
	/// <returns>Null if there is no static save data for the level, otherwise it returns the appropriate static data.</returns>
	public PuzzleStaticSaveData GetPuzzleStaticDataForLevel(string levelId)
	{
		if (!PuzzleStaticDataExistsForLevel(levelId))
		{
			return null;
		}

		return _staticSaveDatas[levelId];
	}

	/// <summary>
	/// Sets the static data of a puzzle to save data.
	/// </summary>
	/// <param name="levelId">The id of the level whose static data is being saved.</param>
	/// <param name="staticData">The data to be saved.</param>
	public void SavePuzzleStaticDataForLevel(string levelId, PuzzleStaticSaveData staticData)
	{
		if (PuzzleStaticDataExistsForLevel(levelId))
		{
			_staticSaveDatas[levelId] = staticData;
		}
		else
		{
			_staticSaveDatas.Add(levelId, staticData);
		}

		WriteOutStaticData();
	}

	/// <summary>
	/// Gets the dynamic save data for a level, if in the save data.
	/// </summary>
	/// <param name="levelId">The id of the level to get the data for.</param>
	/// <returns>Null if there is no dynamic save data for the level, otherwise it returns the appropriate dynamic data.</returns>
	public PuzzleDynamicSaveData GetPuzzleDynamicDataForLevel(string levelId)
	{
		if (!PuzzleDynamicDataExistsForLevel(levelId))
		{
			return null;
		}

		return _dynamicSaveDatas[levelId];
	}

	/// <summary>
	/// Sets the dynamic data of a puzzle to save data.
	/// </summary>
	/// <param name="levelId">The id of the level whose dynamic data is being saved.</param>
	/// <param name="dynamicData">The data to be saved.</param>
	public void SavePuzzleDynamicDataForLevel(string levelId, PuzzleDynamicSaveData dynamicData)
	{
		if (PuzzleDynamicDataExistsForLevel(levelId))
		{
			_dynamicSaveDatas[levelId] = dynamicData;
		}
		else
		{
			_dynamicSaveDatas.Add(levelId, dynamicData);
		}

		WriteOutDynamicData();
	}

	/// <summary>
	/// Removes a puzzle's dynamic and static data from the save data. Namely used when a level has been completed and it no longer needs its in progress data.
	/// </summary>
	/// <param name="levelId">The id of the level to remove.</param>
	public void RemovePuzzleSaveDataForLevel(string levelId)
	{
		bool removed = false;
		if (PuzzleStaticDataExistsForLevel(levelId))
		{
			_staticSaveDatas.Remove(levelId);
			removed = true;
		}

		if (PuzzleDynamicDataExistsForLevel(levelId))
		{
			_dynamicSaveDatas.Remove(levelId);
			removed = true;
		}

		if (removed)
		{
			WriteOutStaticData();
			WriteOutDynamicData();
		}
	}

	/// <summary>
	/// Resets all save data to be empty.
	/// </summary>
	public void ClearAllSaveData()
	{
		_completionData.ResetData();
		_staticSaveDatas.Clear();
		_dynamicSaveDatas.Clear();
		PlayerPrefs.DeleteAll();
	}

	private void ReadInDynamicData()
	{
		if (PlayerPrefs.HasKey(DYNAMIC_DATA_KEY))
		{
			string dynamicDataString = PlayerPrefs.GetString(DYNAMIC_DATA_KEY);
			JSONArray array = JSONNode.Parse(dynamicDataString).AsArray;
			for (int i = 0; i < array.Count; i++)
			{
				string puzzleId = array[i][PUZZLE_ID_KEY].Value;
				PuzzleDynamicSaveData data = new PuzzleDynamicSaveData();
				data.ReadFromJsonString(array[i][DATA_KEY].Value);
				_dynamicSaveDatas.Add(puzzleId, data);
			}
		}
	}

	private void WriteOutDynamicData()
	{
		JSONArray array = new JSONArray();
		foreach (KeyValuePair<string, PuzzleDynamicSaveData> dynamicDataEntry in _dynamicSaveDatas)
		{
			JSONObject node = new JSONObject();
			node[PUZZLE_ID_KEY] = dynamicDataEntry.Key;
			node[DATA_KEY] = dynamicDataEntry.Value.WriteToJsonString();
			array[-1] = node;
		}

		StringBuilder builder = new StringBuilder();
		array.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
		PlayerPrefs.SetString(DYNAMIC_DATA_KEY, builder.ToString());
		PlayerPrefs.Save();
	}

	private void ReadInStaticData()
	{
		if (PlayerPrefs.HasKey(STATIC_DATA_KEY))
		{
			string staticDataString = PlayerPrefs.GetString(STATIC_DATA_KEY);
			JSONArray array = JSONNode.Parse(staticDataString).AsArray;
			for (int i = 0; i < array.Count; i++)
			{
				string puzzleId = array[i][PUZZLE_ID_KEY].Value;
				PuzzleStaticSaveData data = new PuzzleStaticSaveData();
				string dataString = array[i][DATA_KEY].Value;
				data.ReadFromJsonString(dataString);
				_staticSaveDatas.Add(puzzleId, data);
			}
		}
	}

	private void WriteOutStaticData()
	{
		JSONArray array = new JSONArray();
		foreach (KeyValuePair<string, PuzzleStaticSaveData> staticDataEntry in _staticSaveDatas)
		{
			JSONObject node = new JSONObject();
			node[PUZZLE_ID_KEY] = staticDataEntry.Key;
			node[DATA_KEY] = staticDataEntry.Value.WriteToJsonString();
			array[-1] = node;
		}

		StringBuilder builder = new StringBuilder();
		array.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
		PlayerPrefs.SetString(STATIC_DATA_KEY, builder.ToString());
		PlayerPrefs.Save();
	}

	private void ReadInCompletionData()
	{
		if (PlayerPrefs.HasKey(SOLVED_PUZZLES_KEY))
		{
			string completionDataString = PlayerPrefs.GetString(SOLVED_PUZZLES_KEY);
			_completionData.ReadFromJSONString(completionDataString);
		}
	}

	private void WriteOutCompletionData()
	{
		PlayerPrefs.SetString(SOLVED_PUZZLES_KEY, _completionData.WriteToJSONString());
		PlayerPrefs.Save();
	}
}

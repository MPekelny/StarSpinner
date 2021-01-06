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
		// Read in the puzzle completion data.
		if (PlayerPrefs.HasKey(SOLVED_PUZZLES_KEY))
		{
			string completionDataString = PlayerPrefs.GetString(SOLVED_PUZZLES_KEY);
			_completionData.ReadFromJSONString(completionDataString);
		}

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

	public void SaveLevelCompleted(string levelId)
	{
		bool levelWasAdded = _completionData.SetLevelCompleted(levelId);
		if (levelWasAdded)
		{
			WriteOutCompletionData();
		}
	}

	public void RemoveLevelCompleted(string levelId)
	{
		bool levelWasRemoved = _completionData.RemoveLevelCompleted(levelId);
		if (levelWasRemoved)
		{
			WriteOutCompletionData();
		}
	}

	public string GetPuzzleStaticDataForLevel(string levelId)
	{
		if (!PuzzleStaticDataExistsForLevel(levelId))
		{
			return null;
		}

		return _staticSaveDatas[levelId].WriteToJsonString();
	}

	public void SavePuzzleStaticDataForLevel(string levelId, string staticDataJson)
	{
		if (PuzzleStaticDataExistsForLevel(levelId))
		{
			_staticSaveDatas[levelId].ReadFromJsonString(staticDataJson);
		}
		else
		{
			PuzzleStaticSaveData staticData = new PuzzleStaticSaveData();
			staticData.ReadFromJsonString(staticDataJson);
			_staticSaveDatas.Add(levelId, staticData);
		}

		WriteOutStaticData();
	}

	public string GetPuzzleDynamicDataForLevel(string levelId)
	{
		if (!PuzzleDynamicDataExistsForLevel(levelId))
		{
			return null;
		}

		return _dynamicSaveDatas[levelId].WriteToJsonString();
	}

	public void SavePuzzleDynamicDataForLevel(string levelId, string dynamicDataJson)
	{
		if (PuzzleDynamicDataExistsForLevel(levelId))
		{
			_dynamicSaveDatas[levelId].ReadFromJsonString(dynamicDataJson);
		}
		else
		{
			PuzzleDynamicSaveData dynamicData = new PuzzleDynamicSaveData();
			dynamicData.ReadFromJsonString(dynamicDataJson);
			_dynamicSaveDatas.Add(levelId, dynamicData);
		}

		WriteOutDynamicData();
	}

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

	public void ClearAllSaveData()
	{
		_completionData.ResetData();
		_staticSaveDatas.Clear();
		_dynamicSaveDatas.Clear();
		PlayerPrefs.DeleteAll();
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

	private void WriteOutCompletionData()
	{
		PlayerPrefs.SetString(SOLVED_PUZZLES_KEY, _completionData.WriteToJSONString());
		PlayerPrefs.Save();
	}
}

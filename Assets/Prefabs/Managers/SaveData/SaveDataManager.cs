using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
	// So this kind of does not matter since my code is on a public github, but naming the save files like this (as opposed to something like data.sav) provides a layer of obfustication towards players playing around with the save files.
	private const string COMPLETION_SAVE_FILE = "lvlc.sda";
	private const string DYNAMIC_SAVE_FILE = "dyd.sda";
	private const string STATIC_SAVE_FILE = "std.sda";

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
		File.Delete(Application.persistentDataPath + "/" + COMPLETION_SAVE_FILE);
		_staticSaveDatas.Clear();
		File.Delete(Application.persistentDataPath + "/" + STATIC_SAVE_FILE);
		_dynamicSaveDatas.Clear();
		File.Delete(Application.persistentDataPath + "/" + DYNAMIC_SAVE_FILE);
	}

	#region Data Loading/Saving

	private void ReadInDynamicData()
	{
		string filepath = Application.persistentDataPath + "/" + DYNAMIC_SAVE_FILE;
		if (File.Exists(filepath))
		{
			string dynamicDataString = DecryptString(File.ReadAllText(filepath));
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
		string encryptedData = EncryptString(builder.ToString());
		string filepath = Application.persistentDataPath + "/" + DYNAMIC_SAVE_FILE;
		File.WriteAllText(filepath, encryptedData);
	}

	private void ReadInStaticData()
	{
		string filepath = Application.persistentDataPath + "/" + STATIC_SAVE_FILE;
		if (File.Exists(filepath))
		{
			string staticDataString = DecryptString(File.ReadAllText(filepath));
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
		string encryptedData = EncryptString(builder.ToString());
		string filepath = Application.persistentDataPath + "/" + STATIC_SAVE_FILE;
		File.WriteAllText(filepath, encryptedData);
	}

	private void ReadInCompletionData()
	{
		string filepath = Application.persistentDataPath + "/" + COMPLETION_SAVE_FILE;
		if (File.Exists(filepath))
		{
			string completionDataString = DecryptString(File.ReadAllText(filepath));
			_completionData.ReadFromJSONString(completionDataString);
		}
	}

	private void WriteOutCompletionData()
	{
		string encryptedData = EncryptString(_completionData.WriteToJSONString());
		string filepath = Application.persistentDataPath + "/" + COMPLETION_SAVE_FILE;
		File.WriteAllText(filepath, encryptedData);
	}

	#endregion

	#region Encryption/Decryption

	/*
	 * Realistically any save data not stored on a server is not particularily difficult to decrypt if an attacker is interested enough (especially since this code is on a public github).
	 * Making the save files completely secure is not really feasable. So I am just applying a couple basic techniques (base 64ing the json data and then xoring the base 64 string), which 
	 * combined with having a bit of obfustication of the files themselves should at least deter most casual attempts.
	 */

	private string EncryptString(string plainText)
	{
		byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
		string base64String = System.Convert.ToBase64String(plainTextBytes);
		return XOREncryptDecrypt(base64String);
	}

	private string DecryptString(string encryptedString)
	{
		string unXORedString = XOREncryptDecrypt(encryptedString);
		byte[] encodedBytes = System.Convert.FromBase64String(unXORedString);
		return Encoding.UTF8.GetString(encodedBytes);
	}

	private string XOREncryptDecrypt(string stringToTransform)
	{
		char xorKey = 'b';

		StringBuilder processor = new StringBuilder(stringToTransform.Length);
		for (int i = 0; i < stringToTransform.Length; i++)
		{
			char c = (char)(stringToTransform[i] ^ xorKey);
			processor.Append(c);
		}

		return processor.ToString();
	}

	#endregion
}

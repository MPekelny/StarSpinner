using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PuzzleCompletionSaveData
{
	private const string COMPLETED_LEVELS_KEY = "completed_levels";

	private HashSet<string> _levelsCompleted = new HashSet<string>();

	/// <summary>
	/// Sets a level as completed if not done previously.
	/// </summary>
	/// <returns>True if the level id was not already in the data and it was added, false if it was in the data previous.</returns>
	public bool SetLevelCompleted(string levelId)
	{
		if (!_levelsCompleted.Contains(levelId))
		{
			_levelsCompleted.Add(levelId);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Removes a level from being completed if it is in the data.
	/// </summary>
	/// <returns>True if the level id was in the data and it was removed, false if it was not.</returns>
	public bool RemoveLevelCompleted(string levelId)
	{
		if (_levelsCompleted.Contains(levelId))
		{
			_levelsCompleted.Remove(levelId);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Gets if the level had been completed.
	/// </summary>
	/// <returns>If the level had been completed.</returns>
	public bool GetLevelCompleted(string levelId)
	{
		return _levelsCompleted.Contains(levelId);
	}

	/// <summary>
	/// Takes the data and turns it into a json formatted string.
	/// </summary>
	public string WriteToJSONString()
	{
		JSONNode node = new JSONObject();
		foreach (string levelId in _levelsCompleted)
		{
			node[COMPLETED_LEVELS_KEY][-1] = levelId;
		}

		StringBuilder builder = new StringBuilder();
		node.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
		return builder.ToString();
	}

	/// <summary>
	/// Takes a json formatted string and turns it into data.
	/// </summary>
	public void ReadFromJSONString(string json)
	{
		ResetData();

		JSONNode node = JSONNode.Parse(json);
		JSONArray idsArray = node[COMPLETED_LEVELS_KEY].AsArray;
		for (int i = 0; i < idsArray.Count; i++)
		{
			_levelsCompleted.Add(idsArray[i]);
		}
	}

	public void ResetData()
	{
		_levelsCompleted.Clear();
	}
}

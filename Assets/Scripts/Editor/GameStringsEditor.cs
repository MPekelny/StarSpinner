using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameStrings))]
public class GameStringsEditor : Editor
{
	private TextAsset _csvFile = null;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawStringCsvSetter();
		DrawStringsData();
	}

	private void DrawStringCsvSetter()
	{
		GUILayout.Label("Drag a strings csv file to this slot to set the data. This file should contain 2 columns, the first being the keys for the strings and the second being the actual tring for that key.", EditorStyles.wordWrappedLabel);
		_csvFile = (TextAsset)EditorGUILayout.ObjectField(_csvFile, typeof(TextAsset), false);
		if (_csvFile != null)
		{
			List<List<string>> csvData = SimpleCSVParser.ParseCSV(_csvFile.text);
			ParseCSVData(csvData);

			_csvFile = null;
		}
	}

	private void DrawStringsData()
	{
		GUILayout.Space(20f);

		GameStrings gameStrings = (GameStrings)target;
		if (gameStrings.StringValues.Count == 0)
		{
			GUILayout.Label("No Key/Value data currently. Drag a strings csv to the slot above to set it.");
		}
		else
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Keys", EditorStyles.boldLabel, GUILayout.Width(250f));
			GUILayout.Label("Values", EditorStyles.boldLabel);
			GUILayout.EndHorizontal();
			EditorWindowStuff.EditorHelpers.DrawUILine(Color.black, 3, 5);

			foreach (GameStrings.KeyStringPair pair in gameStrings.StringValues)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(pair.Key, EditorStyles.boldLabel, GUILayout.Width(250f));
				GUILayout.Label(pair.Value, EditorStyles.wordWrappedLabel);
				GUILayout.EndHorizontal();
				EditorWindowStuff.EditorHelpers.DrawUILine(Color.black);
			}
		}
	}

	private void ParseCSVData(List<List<string>> csvData)
	{
		if (csvData.Count == 0)
		{
			Debug.LogError("Error parsing strings csv file, it contained no rows.");
			return;
		}

		GameStrings gameStrings = (GameStrings)target;
		gameStrings.ResetKeyStringPairs();

		for (int i = 0; i < csvData.Count; i++)
		{
			List<string> row = csvData[i];
			if (row.Count < 2)
			{
				Debug.LogWarning($"Skipping parsing row {i} of the strings csv as it has less than 2 cells.");
				continue;
			}

			if (string.IsNullOrEmpty(row[0]))
			{
				Debug.LogWarning($"Skipping parsing row {i} of the strings csv as the key for that row is empty.");
				continue;
			}

			GameStrings.KeyStringPair pair = new GameStrings.KeyStringPair(row[0], row[1]);
			gameStrings.AddKeyStringPair(pair);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
}

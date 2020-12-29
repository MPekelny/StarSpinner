using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameData))]
public class GameDataEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(15f);
		GUILayout.Label("This button sorts the puzzle datas so that puzzles with fewer spinners are earlier in the list and ones with more are later.", EditorStyles.wordWrappedLabel);
		GUILayout.Label("Also checks that all puzzle datas are valid; that is they have an id, a name, an appropriate number of spinners and it does not have its id being the same as another puzzles'.", EditorStyles.wordWrappedLabel);
		GUILayout.Label("Any puzzles that fails this condition is removed from the list with a message output into a console log.", EditorStyles.wordWrappedLabel);
		if (GUILayout.Button("Sort and Verify Puzzle Datas"))
		{
			GameData gameData = (GameData)target;
			List<PuzzleData> puzzleData = new List<PuzzleData>(gameData.PuzzleDatas);

			HashSet<string> seenIds = new HashSet<string>();
			for (int i = 0; i < puzzleData.Count; i++)
			{
				PuzzleData data = puzzleData[i];
				// Because of the puzzle tools, we should not have any puzzles in the list that have an invalid id, name or number of spinners and checking if an id is used more than once should be the only thing
				// that is really needed to be checked. But for safety, those things will still be checked.
				if (string.IsNullOrEmpty(data.PuzzleUniqueId))
				{
					LogInvalidPuzzle(data, "No Puzzle ID set.");
					puzzleData.RemoveAt(i--);
					continue;
				}

				if (string.IsNullOrEmpty(data.PuzzleName))
				{
					LogInvalidPuzzle(data, "No Puzzle Name set.");
					puzzleData.RemoveAt(i--);
					continue;
				}

				if (data.NumSpinners < PuzzleData.MIN_NUM_SPINNERS || data.NumSpinners > PuzzleData.MAX_NUM_SPINNERS)
				{
					LogInvalidPuzzle(data, "Invalid number of spinners.");
					puzzleData.RemoveAt(i--);
					continue;
				}

				if (seenIds.Contains(data.PuzzleUniqueId))
				{
					LogInvalidPuzzle(data, "Puzzle ID is used by another puzzle.");
					puzzleData.RemoveAt(i--);
				}
				else
				{
					seenIds.Add(data.PuzzleUniqueId);
				}
			}

			puzzleData = puzzleData.OrderBy(o => o.NumSpinners).ToList();

			gameData.SetSortedPuzzleDatas(puzzleData);
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Debug.Log("Puzzle List sorted and verified.");
		}
	}

	private void LogInvalidPuzzle(PuzzleData data, string reason)
	{
		string puzzleId = string.IsNullOrEmpty(data.PuzzleUniqueId) ? ">Blank<" : data.PuzzleUniqueId;
		string puzzleName = string.IsNullOrEmpty(data.PuzzleName) ? ">Blank<" : data.PuzzleName;

		Debug.LogError($"When verifying Puzzle Data for Puzzle {data.name}, ID: {puzzleId}, Name: {puzzleName} and will be removed from the puzzle list. Reason: {reason}");
	}
}

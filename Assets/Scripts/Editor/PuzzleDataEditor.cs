using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EditorWindowStuff;
using System.IO;

[CustomEditor(typeof(PuzzleData))]
public class PuzzleDataEditor : Editor
{
	private class StarDataEditor
	{
		public Vector3 Position;
		public Color Color;
		public bool FoldedOut;

		public StarDataEditor(Vector3 position, Color color)
		{
			Position = position;
			Color = color;
			FoldedOut = false;
		}
	}

	private string _refImagePath;
	private Texture _refImage = null;

	// Puzzle Datas will only be edited using the tool now, so the inspector for PuzzleDatas will just be for displaying the information.
	// Hence just getting the values instead of using the SerializedProperties.
	private string _puzzleId;
	private string _puzzleName;
	private int _numSpinners;
	private List<StarDataEditor> _starDatas = new List<StarDataEditor>();

	private bool _refImageFoldedOut = false;
	private bool _starDatasFoldedOut = false;

	private void OnEnable()
	{
		_puzzleId = serializedObject.FindProperty("_puzzleUniqueId").stringValue;
		_puzzleName = serializedObject.FindProperty("_puzzleName").stringValue;
		_numSpinners = serializedObject.FindProperty("_numSpinners").intValue;

		SerializedProperty stars = serializedObject.FindProperty("_starDatas");
		for (int i = 0; i < stars.arraySize; i++)
		{
			SerializedProperty property = stars.GetArrayElementAtIndex(i);
			Vector3 posVal = property.FindPropertyRelative("_position").vector3Value;
			Color colVal = property.FindPropertyRelative("_finalColor").colorValue;
			_starDatas.Add(new StarDataEditor(posVal, colVal));
		}

		_refImagePath = serializedObject.FindProperty("_puzzleImageReferencePath").stringValue;
		if (!string.IsNullOrEmpty(_refImagePath))
		{
			if (File.Exists(_refImagePath))
			{
				_refImage = AssetDatabase.LoadAssetAtPath<Texture>(_refImagePath);
			}
		}
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Label("If you wish to edit this puzzle data, you do not use the\ninspector, you load it into the puzzle tool found in\nWindow/Puzzle Editor and make changes there.");
		GUILayout.Space(25f);

		DrawPuzzleDataValues();
		GUILayout.Space(20f);
		DrawRefImageInfo();
	}

	private void DrawRefImageInfo()
	{
		_refImageFoldedOut = EditorGUILayout.Foldout(_refImageFoldedOut, "Editor Reference Image data for puzzle");
		if (_refImageFoldedOut)
		{
			if (_refImage == null)
			{
				if (!string.IsNullOrEmpty(_refImagePath))
				{
					GUILayout.Label($"Reference image for puzzle was set to\n{_refImagePath}\nbut no such image exists there.");
				}
				else
				{
					GUILayout.Label("No reference image set for this puzzle.");
				}
			}
			else
			{
				GUILayout.Label(_refImage, GUILayout.MaxHeight(200f));
				EditorGUILayout.LabelField("File path of reference image:", EditorStyles.boldLabel);
				EditorGUILayout.LabelField(_refImagePath);
			}
		}
	}

	private void DrawPuzzleDataValues()
	{
		EditorGUILayout.LabelField("Data for the current puzzle version.", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorHelpers.DrawPairedLabelFields("Puzzle's Unique id:", _puzzleId);
		EditorHelpers.DrawPairedLabelFields("Puzzle's Name:", _puzzleName);
		EditorHelpers.DrawPairedLabelFields("Puzzle's Spinner Count:", _numSpinners.ToString());

		GUILayout.Space(5f);
		_starDatasFoldedOut = EditorGUILayout.Foldout(_starDatasFoldedOut, "Puzzle Star Datas");
		if (_starDatasFoldedOut)
		{
			EditorGUI.indentLevel++;
			EditorHelpers.DrawPairedLabelFields("Number of Stars:", _starDatas.Count.ToString());
			for (int i = 0; i < _starDatas.Count; i++)
			{
				_starDatas[i].FoldedOut = EditorGUILayout.Foldout(_starDatas[i].FoldedOut, $"Star {i + 1}");
				if (_starDatas[i].FoldedOut)
				{
					EditorGUI.indentLevel++;
					EditorHelpers.DrawPairedLabelFields("Position in Puzzle:", $"(X: {_starDatas[i].Position.x}, Y: {_starDatas[i].Position.y})");
					EditorHelpers.DrawColorDisplay("Final Color: ", _starDatas[i].Color);
					EditorGUI.indentLevel--;
				}
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.indentLevel--;
	}
}

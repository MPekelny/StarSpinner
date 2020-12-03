using UnityEditor;
using UnityEngine;

namespace EditorWindowStuff
{
	public static class EditorHelpers
	{
		/// <summary>
		/// Draws a pair of labels side by side with the left item being bolded.
		/// </summary>
		public static void DrawPairedLabelFields(string leftField, string rightField)
		{
			GUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(leftField, EditorStyles.boldLabel);
			EditorGUILayout.LabelField(rightField);

			GUILayout.EndHorizontal();
		}

		public static void DrawColorDisplay(string label, Color displayColor)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			GUI.enabled = false;
			EditorGUILayout.ColorField(displayColor);
			GUI.enabled = true;
			GUILayout.EndHorizontal();
		}
	}
}


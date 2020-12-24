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

		/// <summary>
		/// Draws a horizontal line in the editor. Copied from alexanderameye's post at https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/
		/// </summary>
		public static void DrawUILine(Color color, int thickness = 2, int padding = 2)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, color);
		}


		/// <summary>
		/// Draws a text label with a color field beide it that is disabled so that it only displays the colour and does not allow it to be edited.
		/// </summary>
		public static void DrawColorDisplay(string label, Color displayColor)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
			GUI.enabled = false;
			EditorGUILayout.ColorField(displayColor);
			GUI.enabled = true;
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Draws a button that, if the enabled parameter is false, disables the gui for the button.
		/// </summary>
		/// <returns>The pressed state of the button.</returns>
		public static bool DrawDisablableButton(string buttonText, float buttonWidth, bool enabled)
		{
			bool retVal = false;

			GUI.enabled = enabled;
			retVal = GUILayout.Button(buttonText, GUILayout.Width(buttonWidth));
			GUI.enabled = true;

			return retVal;
		}
	}
}


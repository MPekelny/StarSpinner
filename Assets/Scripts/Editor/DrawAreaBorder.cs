using UnityEditor;
using UnityEngine;

namespace EditorWindowStuff
{
	public class DrawAreaBorder
	{
		Vector3[] _borderCorners = new Vector3[4];
		Color _borderColor = Color.black;

		public DrawAreaBorder(Rect drawArea, Color borderColor)
		{
			_borderCorners[0] = new Vector3(drawArea.xMin, drawArea.yMin);
			_borderCorners[1] = new Vector3(drawArea.xMin, drawArea.yMax);
			_borderCorners[2] = new Vector3(drawArea.xMax, drawArea.yMax);
			_borderCorners[3] = new Vector3(drawArea.xMax, drawArea.yMin);

			_borderColor = borderColor;
		}

		public void Draw()
		{
			Handles.BeginGUI();
			Handles.color = _borderColor;
			Handles.DrawLine(_borderCorners[0], _borderCorners[1]);
			Handles.DrawLine(_borderCorners[1], _borderCorners[2]);
			Handles.DrawLine(_borderCorners[2], _borderCorners[3]);
			Handles.DrawLine(_borderCorners[3], _borderCorners[0]);

			Handles.EndGUI();
		}
	}
}

using UnityEditor;
using UnityEngine;

namespace EditorWindowStuff
{
	public class EditorWindowImage
	{
		public Texture Texture { get; set; }
		public Color Color { get; set; }
		public bool Visible { get; set; } = true;
		private Vector2 _position = Vector2.zero;
		public Vector2 Position
		{
			get { return _position; }
			set
			{
				_position = value;
				RecreateDrawRect();
			}
		}

		private float _width = 0f;
		public float Width
		{
			get { return _width; }
			set
			{
				_width = value;
				RecreateDrawRect();
			}
		}

		private float _height = 0f;
		public float Height
		{
			get { return _height; }
			set
			{
				_height = value;
				RecreateDrawRect();
			}
		}

		private Rect _drawRect;
		private ScaleMode ScaleMode { get; set; } = ScaleMode.ScaleToFit;

		public EditorWindowImage(Texture drawTexture, float width, float height, Vector2 drawPosition, Color color)
		{
			Texture = drawTexture;
			Color = color;
			_position = drawPosition;
			_width = width;
			_height = height;
			RecreateDrawRect();
		}

		public void Draw()
		{
			if (!Visible) return;

			if (Texture == null)
			{
				EditorGUI.DrawRect(_drawRect, Color);
			}
			else
			{
				GUI.DrawTexture(_drawRect, Texture, ScaleMode, true, 0f, Color, 0f, 0f);
			}
		}

		private void RecreateDrawRect()
		{
			_drawRect = new Rect(_position.x - _width / 2f,
								 _position.y - _height / 2f,
								 _width,
								 _height);
		}
	}
}


using UnityEditor;
using UnityEngine;

namespace EditorWindowStuff
{
	public class PuzzleEditorStar
	{
		private static Texture starImage = null;
		private const float WIDTH = 10f;
		private const float RADIUS = WIDTH / 2f;

		private Vector2 _gamePosition = Vector2.zero;
		private Vector2 _editorPosition = Vector2.zero;
		private Color _endColour = Color.white;
		private Rect _drawAreaReference;

		public Vector2 GamePosition => _gamePosition;
		public Vector2 EditorPosition => _editorPosition;

		public Color EndColour
		{
			get { return _endColour; }
			set { _endColour = value; }
		}

		public PuzzleEditorStar(Color endColour, Rect drawAreaRef)
		{
			_endColour = endColour;
			_drawAreaReference = drawAreaRef;
		}

		/// <summary>
		/// Given an editor position (that is, a spot in the editor window) it will set that position as well as translate it to a game position, which is the version of the position
		/// that the game uses, relative to the center of the area where stars are placed,
		/// </summary>
		public void SetPositionsUsingEditorPosiiton(Vector2 editorPosition)
		{
			_editorPosition = editorPosition;
			_gamePosition = editorPosition - _drawAreaReference.center;
			_gamePosition.y *= -1f;
		}

		/// <summary>
		/// Basically the same as the other position setter, just setting the editor position from the game position. Mainly used for setting up a star that is loaded from an existing puzzle
		/// (where we have the game position but would thus need to get the editor position from that).
		/// </summary>
		public void SetPositionUsingGamePosition(Vector2 gamePosition)
		{
			_gamePosition = gamePosition;
			_editorPosition = gamePosition;
			_editorPosition.y *= -1f;
			_editorPosition += _drawAreaReference.center;
		}

		public bool OverlapsPoint(Vector2 point)
		{
			Vector2 distanceBetweenPointAndStar = point - _editorPosition;

			return distanceBetweenPointAndStar.sqrMagnitude < RADIUS * RADIUS;
		}

		public bool WithinRangeOfPoint(Vector2 point, float rangeRadius)
		{
			// Slightly different one, used for getting if the position is not within the radius of the star, but within a different amount from the star.
			// Namely for when in select mode for when there is a selected star, the star is to be considered still selected a bit farther out from the star.
			Vector2 distanceBetweenPointAndStar = point - _editorPosition;

			return distanceBetweenPointAndStar.sqrMagnitude < rangeRadius * rangeRadius;
		}

		public bool OverlapsStar(PuzzleEditorStar otherStar)
		{
			Vector2 distanceBetweenStars = otherStar.EditorPosition - EditorPosition;

			return distanceBetweenStars.sqrMagnitude < WIDTH * WIDTH;
		}

		public void Draw()
		{
			if (starImage == null)
			{
				starImage = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Content/Art/StarImage.png");
			}

			GUI.DrawTexture(GetDrawRectForPosition(_editorPosition), starImage, ScaleMode.StretchToFill, true, 0f, _endColour, 0f, 0f);
		}

		private Rect GetDrawRectForPosition(Vector2 pos)
		{
			return new Rect(pos.x - RADIUS,
							pos.y - RADIUS,
							WIDTH,
							WIDTH);
		}
	}
}

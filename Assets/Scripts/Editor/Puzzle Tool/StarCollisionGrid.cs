using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorWindowStuff
{
	public class StarCollisionGrid
	{
		private struct GridPosition
		{
			public int gridX;
			public int gridY;
		}

		private struct GridCheckRanges
		{
			public int checkXStart;
			public int checkXEnd;
			public int checkYStart;
			public int checkYEnd;
		}

		private PuzzleEditorStar[,] _starGrid;
		private Rect _starAreaReference;

		public StarCollisionGrid(Rect areaReference)
		{
			_starAreaReference = areaReference;
			int gridWidth = Mathf.FloorToInt(areaReference.width / PuzzleEditorStar.WIDTH);
			int gridHeight = Mathf.FloorToInt(areaReference.height / PuzzleEditorStar.WIDTH);
			_starGrid = new PuzzleEditorStar[gridWidth, gridHeight];
		}

		/// <summary>
		/// Tries to add the star to the grid at the equivelant grid spot.
		/// </summary>
		/// <returns>True if the star was added, false if it couldn't (namely if the intended position already has a star at its location or the position is outside the area).</returns>
		public bool SetStarToGrid(PuzzleEditorStar star)
		{
			if (!PointValidForGrid(star.EditorPosition))
			{
				Debug.LogError($"Called StarCollisionGrid.SetStarToGrid with a star whose position is not inside the area of the collision grid. Point: {star.EditorPosition}, grid area: {_starAreaReference}");
				return false;
			}

			GridPosition position = GetGridPositionForPoint(star.EditorPosition);
			if (_starGrid[position.gridX, position.gridY] == null)
			{
				_starGrid[position.gridX, position.gridY] = star;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Tries to remove the star from the grid at the equivelant grid spot.
		/// </summary>
		/// <returns>True if a star at the grid spot was removed, false if it couldn't (there was no star at the grid spot or the point is outside the grid area).</returns>
		public bool PullStarFromGridAtPoint(Vector2 point)
		{
			if (!PointValidForGrid(point))
			{
				Debug.LogError($"Called StarCollisionGrid.PullStarFromGridAtPoint with a point not inside the area of the collision grid. Point: {point}, grid area: {_starAreaReference}");
				return false;
			}

			GridPosition position = GetGridPositionForPoint(point);
			if (_starGrid[position.gridX, position.gridY] != null)
			{
				_starGrid[position.gridX, position.gridY] = null;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Given a position inside the grid area, gets a star that overlaps that point if there is one.
		/// </summary>
		public PuzzleEditorStar GetStarAtPoint(Vector2 point)
		{
			if (!PointValidForGrid(point))
			{
				Debug.LogError($"Called StarCollisionGrid.GetStarAtPoint with a point not inside the area of the collision grid. Point: {point}, grid area: {_starAreaReference}");
				return null;
			}

			GridPosition position = GetGridPositionForPoint(point);
			GridCheckRanges ranges = GetCheckRangesForPosition(position);
			for (int gridX = ranges.checkXStart; gridX <= ranges.checkXEnd; gridX++)
			{
				for (int gridY = ranges.checkYStart; gridY <= ranges.checkYEnd; gridY++)
				{
					if (_starGrid[gridX, gridY] != null)
					{
						if (_starGrid[gridX, gridY].OverlapsPoint(point))
						{
							return _starGrid[gridX, gridY];
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// A method to get if a star would overlap with a star in the grid. It just uses a position and assumes a radius of the star. This way you can check for overlap without having actually created a star.
		/// </summary>
		public bool StarAreaOverlapsStar(Vector2 point)
		{
			if (!PointValidForGrid(point))
			{
				Debug.LogError($"Called StarCollisionGrid.StarAreaOverlapsStar with a point not inside the area of the collision grid. Point: {point}, grid area: {_starAreaReference}");
				return false;
			}

			GridPosition position = GetGridPositionForPoint(point);
			GridCheckRanges ranges = GetCheckRangesForPosition(position);
			for (int gridX = ranges.checkXStart; gridX <= ranges.checkXEnd; gridX++)
			{
				for (int gridY = ranges.checkYStart; gridY <= ranges.checkYEnd; gridY++)
				{
					if (_starGrid[gridX, gridY] != null)
					{
						Vector2 distance = _starGrid[gridX, gridY].EditorPosition - point;
						if (distance.sqrMagnitude <= PuzzleEditorStar.WIDTH * PuzzleEditorStar.WIDTH)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public void ClearGrid()
		{
			for (int gridX = 0; gridX < _starGrid.GetLength(0); gridX++)
			{
				for (int gridY = 0; gridY < _starGrid.GetLength(1); gridY++)
				{
					_starGrid[gridX, gridY] = null;
				}
			}
		}

		private GridPosition GetGridPositionForPoint(Vector2 point)
		{
			// The position ned to be adjusted to be relative to the reference area.
			Vector2 rectAdjustedPosition = new Vector2(point.x - _starAreaReference.xMin, point.y - _starAreaReference.yMin);

			GridPosition position;
			position.gridX = Mathf.FloorToInt(rectAdjustedPosition.x / PuzzleEditorStar.WIDTH);
			position.gridY = Mathf.FloorToInt(rectAdjustedPosition.y / PuzzleEditorStar.WIDTH);

			return position;
		}

		private GridCheckRanges GetCheckRangesForPosition(GridPosition position)
		{
			GridCheckRanges ranges;
			ranges.checkXStart = Mathf.Max(position.gridX - 1, 0);
			ranges.checkXEnd = Mathf.Min(position.gridX + 1, _starGrid.GetLength(0) - 1);
			ranges.checkYStart = Mathf.Max(position.gridY - 1, 0);
			ranges.checkYEnd = Mathf.Min(position.gridY + 1, _starGrid.GetLength(1) - 1);

			return ranges;
		}

		private bool PointValidForGrid(Vector2 point)
		{
			bool xValid = point.x >= _starAreaReference.xMin && point.x <= _starAreaReference.xMax;
			bool yValid = point.y >= _starAreaReference.yMin && point.y <= _starAreaReference.yMax;

			return xValid && yValid;
		}
	}
}


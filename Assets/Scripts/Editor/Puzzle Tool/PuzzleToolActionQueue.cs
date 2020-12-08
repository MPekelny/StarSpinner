using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorWindowStuff
{
	public abstract class PuzzleToolAction
	{
		public abstract void Undo();
		public abstract void Redo();
	}

	#region PuzzleAddStarAction
	public class PuzzleAddStarAction : PuzzleToolAction
	{
		private List<PuzzleEditorStar> _starList;
		private StarCollisionGrid _starGrid;
		private Rect _drawAreaRef;
		private Vector2 _starAddedPosition;
		private Color _starAddedColor;

		public PuzzleAddStarAction(List<PuzzleEditorStar> starList, StarCollisionGrid grid, PuzzleEditorStar addedStar)
		{
			_starList = starList;
			_starGrid = grid;
			_drawAreaRef = addedStar.DrawAreaReference;
			_starAddedPosition = addedStar.EditorPosition;
			_starAddedColor = addedStar.EndColour;
		}

		public override void Undo()
		{
			if (!_starGrid.PullStarFromGridAtPoint(_starList[_starList.Count - 1].EditorPosition))
			{
				Debug.LogError("Something went wrong undoing PuzzleAddStarAction, PullStarFromGrid failed.");
			}

			_starList.RemoveAt(_starList.Count - 1);
		}

		public override void Redo()
		{
			PuzzleEditorStar star = new PuzzleEditorStar(_starAddedColor, _drawAreaRef);
			star.SetPositionsUsingEditorPosiiton(_starAddedPosition);
			if (!_starGrid.SetStarToGrid(star))
			{
				Debug.LogError("Something went wrong redoing PuzzleAddStarAction, SetStarToGrid failed.");
			}

			_starList.Add(star);
		}
	}
	#endregion

	#region PuzzleDeleteStarAction
	public class PuzzleDeleteStarAction : PuzzleToolAction
	{
		private List<PuzzleEditorStar> _starList;
		private StarCollisionGrid _starGrid;
		private Rect _drawAreaRef;
		private Vector2 _starDeletedPosition;
		private Color _starDeletedColor;
		private int _starDeletedIndex;

		public PuzzleDeleteStarAction(List<PuzzleEditorStar> starList, StarCollisionGrid grid, PuzzleEditorStar deletedStar, int deletedStarIndex)
		{
			_starList = starList;
			_starGrid = grid;
			_drawAreaRef = deletedStar.DrawAreaReference;
			_starDeletedPosition = deletedStar.EditorPosition;
			_starDeletedColor = deletedStar.EndColour;
			_starDeletedIndex = deletedStarIndex;
		}

		public override void Undo()
		{
			PuzzleEditorStar star = new PuzzleEditorStar(_starDeletedColor, _drawAreaRef);
			star.SetPositionsUsingEditorPosiiton(_starDeletedPosition);
			if (!_starGrid.SetStarToGrid(star))
			{
				Debug.LogError("Something went wrong undoing PuzzleDeleteStarAction, SetStarToGrid failed.");
			}

			_starList.Insert(_starDeletedIndex, star);
		}

		public override void Redo()
		{
			if (!_starGrid.PullStarFromGridAtPoint(_starList[_starDeletedIndex].EditorPosition))
			{
				Debug.LogError("Something went wrong redoing PuzzleDeleteStarAction, PullStarFromGrid failed.");
			}

			_starList.RemoveAt(_starDeletedIndex);
		}
	}
	#endregion

	#region PuzzleMoveStarAction
	public class PuzzleMoveStarAction : PuzzleToolAction
	{
		private List<PuzzleEditorStar> _starList;
		private StarCollisionGrid _starGrid;
		private int _starChangedIndex;
		private Vector2 _positionBefore;
		private Vector2 _positionAfter;

		public PuzzleMoveStarAction(List<PuzzleEditorStar> starList, StarCollisionGrid grid, int starChangedIndex, Vector2 positionBefore, Vector2 positionAfter)
		{
			_starList = starList;
			_starGrid = grid;
			_starChangedIndex = starChangedIndex;
			_positionBefore = positionBefore;
			_positionAfter = positionAfter;
		}

		public override void Undo()
		{
			if (!_starGrid.PullStarFromGridAtPoint(_positionAfter))
			{
				Debug.LogError("Something went wrong undoing PuzzleMoveStarAction, PullStarFromGrid failed.");
			}

			_starList[_starChangedIndex].SetPositionsUsingEditorPosiiton(_positionBefore);

			if (!_starGrid.SetStarToGrid(_starList[_starChangedIndex]))
			{
				Debug.LogError("Something went wrong undoing PuzzleMoveStarAction, SetStarToGrid failed.");
			}
		}

		public override void Redo()
		{
			if (!_starGrid.PullStarFromGridAtPoint(_positionBefore))
			{
				Debug.LogError("Something went wrong redoing PuzzleMoveStarAction, PullStarFromGrid failed.");
			}

			_starList[_starChangedIndex].SetPositionsUsingEditorPosiiton(_positionAfter);

			if (!_starGrid.SetStarToGrid(_starList[_starChangedIndex]))
			{
				Debug.LogError("Something went wrong redoing PuzzleMoveStarAction, SetStarToGrid failed.");
			}
		}
	}
	#endregion

	#region PuzzleColorStarAction
	public class PuzzleColorStarAction : PuzzleToolAction
	{
		private List<PuzzleEditorStar> _starList;
		private int _starChangedIndex;
		private Color _colorBefore;
		private Color _colorAfter;

		public PuzzleColorStarAction(List<PuzzleEditorStar> starList, int starChangedIndex, Color colorBefore, Color colorAfter)
		{
			_starList = starList;
			_starChangedIndex = starChangedIndex;
			_colorBefore = colorBefore;
			_colorAfter = colorAfter;
		}

		public override void Undo()
		{
			_starList[_starChangedIndex].EndColour = _colorBefore;
		}

		public override void Redo()
		{
			_starList[_starChangedIndex].EndColour = _colorAfter;
		}
	}
	#endregion
	
	public class PuzzleToolActionQueue
	{
		// An action that does nothing so that there is a blank action to be the start of the action queue.
		// Realistically, I could just use the base PuzzleToolAction, but I want to keep that abstract and this makes it completely clear this is a blank action meant to do nothing.
		private class BlankAction : PuzzleToolAction
		{
			public override void Undo() { }
			public override void Redo() { }
		}

		private List<PuzzleToolAction> _actions = new List<PuzzleToolAction>();
		private int _currentActionIndex;

		// There are actions available to be undone if we are not at the start of the queue and there are actions available to be redone if we are not at the end of the action queue.
		public bool UndoActionsAvailable => _currentActionIndex > 0;
		public bool RedoActionsAvailable => _currentActionIndex < _actions.Count - 1;

		public PuzzleToolActionQueue()
		{
			BlankAction blank = new BlankAction();
			_actions.Add(blank);
			_currentActionIndex = 0;
		}

		/// <summary>
		/// If the current action is not the last item in the queue, gets rid of all actions after the current action.
		/// Then, adds an action to the queue and makes that action the current action.
		/// </summary>
		public void AddAction(PuzzleToolAction action)
		{
			if (RedoActionsAvailable)
			{
				_actions.RemoveRange(_currentActionIndex + 1, _actions.Count - (_currentActionIndex + 1));
			}

			_actions.Add(action);
			_currentActionIndex++;
		}

		/// <summary>
		/// Resets the queue back to its initial state.
		/// </summary>
		public void ClearQueue()
		{
			_actions.RemoveRange(1, _actions.Count - 1);
			_currentActionIndex = 0;
		}

		/// <summary>
		/// If there are any actions available to be undone, undoes the current action and makes the previous action the current action.
		/// </summary>
		public void UndoAction()
		{
			if (UndoActionsAvailable)
			{
				_actions[_currentActionIndex].Undo();
				_currentActionIndex--;
			}
		}

		/// <summary>
		/// If there are any actions available to be redone, redoes the next action and makes the next action the current action.
		/// </summary>
		public void RedoAction()
		{
			if (RedoActionsAvailable)
			{
				_currentActionIndex++;
				_actions[_currentActionIndex].Redo();
			}
		}
	}
}


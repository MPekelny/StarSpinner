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
		private Rect _drawAreaRef;
		private Vector2 _starAddedPosition;
		private Color _starAddedColor;

		public PuzzleAddStarAction(List<PuzzleEditorStar> starList, PuzzleEditorStar addedStar)
		{
			_starList = starList;
			_drawAreaRef = addedStar.DrawAreaReference;
			_starAddedPosition = addedStar.EditorPosition;
			_starAddedColor = addedStar.EndColour;
		}

		public override void Undo()
		{
			_starList.RemoveAt(_starList.Count - 1);
		}

		public override void Redo()
		{
			PuzzleEditorStar star = new PuzzleEditorStar(_starAddedColor, _drawAreaRef);
			star.SetPositionsUsingEditorPosiiton(_starAddedPosition);
			_starList.Add(star);
		}
	}
	#endregion

	#region PuzzleDeleteStarAction
	public class PuzzleDeleteStarAction : PuzzleToolAction
	{
		private List<PuzzleEditorStar> _starList;
		private Rect _drawAreaRef;
		private Vector2 _starDeletedPosition;
		private Color _starDeletedColor;
		private int _starDeletedIndex;

		public PuzzleDeleteStarAction(List<PuzzleEditorStar> starList, PuzzleEditorStar deletedStar, int deletedStarIndex)
		{
			_starList = starList;
			_drawAreaRef = deletedStar.DrawAreaReference;
			_starDeletedPosition = deletedStar.EditorPosition;
			_starDeletedColor = deletedStar.EndColour;
			_starDeletedIndex = deletedStarIndex;
		}

		public override void Undo()
		{
			PuzzleEditorStar star = new PuzzleEditorStar(_starDeletedColor, _drawAreaRef);
			star.SetPositionsUsingEditorPosiiton(_starDeletedPosition);
			_starList.Insert(_starDeletedIndex, star);
		}

		public override void Redo()
		{
			_starList.RemoveAt(_starDeletedIndex);
		}
	}
	#endregion

	#region PuzzleMoveStarAction
	public class PuzzleMoveStarAction : PuzzleToolAction
	{
		private List<PuzzleEditorStar> _starList;
		private int _starChangedIndex;
		private Vector2 _positionBefore;
		private Vector2 _positionAfter;

		public PuzzleMoveStarAction(List<PuzzleEditorStar> starList, int starChangedIndex, Vector2 positionBefore, Vector2 positionAfter)
		{
			_starList = starList;
			_starChangedIndex = starChangedIndex;
			_positionBefore = positionBefore;
			_positionAfter = positionAfter;
		}

		public override void Undo()
		{
			_starList[_starChangedIndex].SetPositionsUsingEditorPosiiton(_positionBefore);
		}

		public override void Redo()
		{
			_starList[_starChangedIndex].SetPositionsUsingEditorPosiiton(_positionAfter);
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


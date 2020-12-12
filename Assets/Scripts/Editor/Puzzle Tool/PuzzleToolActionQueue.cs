using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorWindowStuff
{
	public abstract class PuzzleToolAction
	{
		public const string TYPE_KEY = "type";
		public const string INDEX_KEY = "index";

		public abstract void Undo();
		public abstract void Redo();
		public abstract JSONObject GetDataAsJSONNode();
	}

	#region PuzzleAddStarAction
	public class PuzzleAddStarAction : PuzzleToolAction
	{
		public const string ACTION_TYPE = "add";
		private const string POS_X_KEY = "added_pos_x";
		private const string POS_Y_KEY = "added_pos_y";
		private const string COL_R_KEY = "added_col_r";
		private const string COL_G_KEY = "added_col_g";
		private const string COL_B_KEY = "added_col_b";
		private const string AREA_REF_X = "draw_area_x";
		private const string AREA_REF_Y = "draw_area_y";
		private const string AREA_REF_WIDTH = "draw_area_width";
		private const string AREA_REF_HEIGHT = "draw_area_height";

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

		public PuzzleAddStarAction(List<PuzzleEditorStar> starList, StarCollisionGrid grid, JSONNode node)
		{
			_starList = starList;
			_starGrid = grid;
			_starAddedPosition = new Vector2(node[POS_X_KEY].AsFloat, node[POS_Y_KEY].AsFloat);
			_starAddedColor = new Color(node[COL_R_KEY].AsFloat, node[COL_G_KEY].AsFloat, node[COL_B_KEY].AsFloat);
			_drawAreaRef = new Rect(node[AREA_REF_X].AsFloat, node[AREA_REF_Y].AsFloat, node[AREA_REF_WIDTH].AsFloat, node[AREA_REF_HEIGHT].AsFloat);
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

		public override JSONObject GetDataAsJSONNode()
		{
			JSONObject node = new JSONObject();
			node[TYPE_KEY] = ACTION_TYPE;
			node[POS_X_KEY] = _starAddedPosition.x;
			node[POS_Y_KEY] = _starAddedPosition.y;
			node[COL_R_KEY] = _starAddedColor.r;
			node[COL_G_KEY] = _starAddedColor.g;
			node[COL_B_KEY] = _starAddedColor.b;
			node[AREA_REF_X] = _drawAreaRef.x;
			node[AREA_REF_Y] = _drawAreaRef.y;
			node[AREA_REF_WIDTH] = _drawAreaRef.width;
			node[AREA_REF_HEIGHT] = _drawAreaRef.height;

			return node;
		}
	}
	#endregion

	#region PuzzleDeleteStarAction
	public class PuzzleDeleteStarAction : PuzzleToolAction
	{
		public const string ACTION_TYPE = "delete";
		private const string POS_X_KEY = "deleted_pos_x";
		private const string POS_Y_KEY = "deleted_pos_y";
		private const string COL_R_KEY = "deleted_col_r";
		private const string COL_G_KEY = "deleted_col_g";
		private const string COL_B_KEY = "deleted_col_b";
		private const string AREA_REF_X = "draw_area_x";
		private const string AREA_REF_Y = "draw_area_y";
		private const string AREA_REF_WIDTH = "draw_area_width";
		private const string AREA_REF_HEIGHT = "draw_area_height";

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

		public PuzzleDeleteStarAction(List<PuzzleEditorStar> starList, StarCollisionGrid grid, JSONNode node)
		{
			_starList = starList;
			_starGrid = grid;
			_starDeletedPosition = new Vector2(node[POS_X_KEY].AsFloat, node[POS_Y_KEY].AsFloat);
			_starDeletedColor = new Color(node[COL_R_KEY].AsFloat, node[COL_G_KEY].AsFloat, node[COL_B_KEY].AsFloat);
			_drawAreaRef = new Rect(node[AREA_REF_X].AsFloat, node[AREA_REF_Y].AsFloat, node[AREA_REF_WIDTH].AsFloat, node[AREA_REF_HEIGHT].AsFloat);
			_starDeletedIndex = node[INDEX_KEY].AsInt;
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

		public override JSONObject GetDataAsJSONNode()
		{
			JSONObject node = new JSONObject();
			node[TYPE_KEY] = ACTION_TYPE;
			node[INDEX_KEY] = _starDeletedIndex;
			node[POS_X_KEY] = _starDeletedPosition.x;
			node[POS_Y_KEY] = _starDeletedPosition.y;
			node[COL_R_KEY] = _starDeletedColor.r;
			node[COL_G_KEY] = _starDeletedColor.g;
			node[COL_B_KEY] = _starDeletedColor.b;
			node[AREA_REF_X] = _drawAreaRef.x;
			node[AREA_REF_Y] = _drawAreaRef.y;
			node[AREA_REF_WIDTH] = _drawAreaRef.width;
			node[AREA_REF_HEIGHT] = _drawAreaRef.height;

			return node;
		}
	}
	#endregion

	#region PuzzleMoveStarAction
	public class PuzzleMoveStarAction : PuzzleToolAction
	{
		public const string ACTION_TYPE = "move";
		private const string PREV_POS_X_KEY = "prev_pos_x";
		private const string PREV_POS_Y_KEY = "prev_pos_y";
		private const string AFTER_POS_X_KEY = "prev_pos_y";
		private const string AFTER_POS_Y_KEY = "prev_pos_y";

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

		public PuzzleMoveStarAction(List<PuzzleEditorStar> starList, StarCollisionGrid grid, JSONNode node)
		{
			_starList = starList;
			_starGrid = grid;
			_starChangedIndex = node[INDEX_KEY].AsInt;
			_positionBefore = new Vector2(node[PREV_POS_X_KEY].AsFloat, node[PREV_POS_Y_KEY].AsFloat);
			_positionAfter = new Vector2(node[AFTER_POS_X_KEY].AsFloat, node[AFTER_POS_Y_KEY].AsFloat);
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

		public override JSONObject GetDataAsJSONNode()
		{
			JSONObject node = new JSONObject();
			node[TYPE_KEY] = ACTION_TYPE;
			node[INDEX_KEY] = _starChangedIndex;
			node[PREV_POS_X_KEY] = _positionBefore.x;
			node[PREV_POS_Y_KEY] = _positionBefore.y;
			node[AFTER_POS_X_KEY] = _positionAfter.x;
			node[AFTER_POS_Y_KEY] = _positionAfter.y;

			return node;
		}
	}
	#endregion

	#region PuzzleColorStarAction
	public class PuzzleColorStarAction : PuzzleToolAction
	{
		public const string ACTION_TYPE = "color";
		private const string BEFORE_COLOR_R_KEY = "before_col_r";
		private const string BEFORE_COLOR_G_KEY = "before_col_g";
		private const string BEFORE_COLOR_B_KEY = "before_col_b";
		private const string AFTER_COLOR_R_KEY = "after_col_r";
		private const string AFTER_COLOR_G_KEY = "after_col_g";
		private const string AFTER_COLOR_B_KEY = "after_col_b";

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

		public PuzzleColorStarAction(List<PuzzleEditorStar> starList, JSONNode node)
		{
			_starList = starList;
			_starChangedIndex = node[INDEX_KEY].AsInt;
			_colorBefore = new Color(node[BEFORE_COLOR_R_KEY].AsFloat, node[BEFORE_COLOR_G_KEY].AsFloat, node[BEFORE_COLOR_B_KEY].AsFloat);
			_colorAfter = new Color(node[AFTER_COLOR_R_KEY].AsFloat, node[AFTER_COLOR_G_KEY].AsFloat, node[AFTER_COLOR_B_KEY].AsFloat);
		}

		public override void Undo()
		{
			_starList[_starChangedIndex].EndColour = _colorBefore;
		}

		public override void Redo()
		{
			_starList[_starChangedIndex].EndColour = _colorAfter;
		}

		public override JSONObject GetDataAsJSONNode()
		{
			JSONObject node = new JSONObject();
			node[TYPE_KEY] = ACTION_TYPE;
			node[INDEX_KEY] = _starChangedIndex;
			node[BEFORE_COLOR_R_KEY] = _colorBefore.r;
			node[BEFORE_COLOR_G_KEY] = _colorBefore.g;
			node[BEFORE_COLOR_B_KEY] = _colorBefore.b;
			node[AFTER_COLOR_R_KEY] = _colorAfter.r;
			node[AFTER_COLOR_G_KEY] = _colorAfter.g;
			node[AFTER_COLOR_B_KEY] = _colorAfter.b;

			return node;
		}
	}
	#endregion
	
	public class PuzzleToolActionQueue
	{
		private const string CURRENT_INDEX_KEY = "current_index";
		private const string ACTIONS_KEY = "actions";

		// An action that does nothing so that there is a blank action to be the start of the action queue.
		// Realistically, I could just use the base PuzzleToolAction, but I want to keep that abstract and this makes it completely clear this is a blank action meant to do nothing.
		private class BlankAction : PuzzleToolAction
		{
			public override void Undo() { }
			public override void Redo() { }
			public override JSONObject GetDataAsJSONNode() { return null; }
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

		public JSONObject GetQueueDataAsNode()
		{
			JSONObject node = new JSONObject();
			node[CURRENT_INDEX_KEY] = _currentActionIndex;
			node[ACTIONS_KEY] = new JSONArray();

			// Skip the first action as that is the black action to make sure the queue works.
			for (int i = 1; i < _actions.Count; i++)
			{
				node[ACTIONS_KEY][-1] = _actions[i].GetDataAsJSONNode();
			}

			return node;
		}

		public void SetDataFromNode(JSONNode node, List<PuzzleEditorStar> starList, StarCollisionGrid grid)
		{
			_currentActionIndex = node[CURRENT_INDEX_KEY].AsInt;
			JSONArray actions = node[ACTIONS_KEY].AsArray;

			for (int i = 0; i < actions.Count; i++)
			{
				string actionType = actions[i][PuzzleToolAction.TYPE_KEY];
				switch (actionType)
				{
					case PuzzleAddStarAction.ACTION_TYPE:
						_actions.Add(new PuzzleAddStarAction(starList, grid, actions[i]));
						break;

					case PuzzleDeleteStarAction.ACTION_TYPE:
						_actions.Add(new PuzzleDeleteStarAction(starList, grid, actions[i]));
						break;

					case PuzzleMoveStarAction.ACTION_TYPE:
						_actions.Add(new PuzzleMoveStarAction(starList, grid, actions[i]));
						break;

					case PuzzleColorStarAction.ACTION_TYPE:
						_actions.Add(new PuzzleColorStarAction(starList, actions[i]));
						break;
				}
			}
		}
	}
}


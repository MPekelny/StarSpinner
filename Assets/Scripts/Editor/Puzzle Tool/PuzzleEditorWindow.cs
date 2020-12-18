using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorWindowStuff
{
	public class PuzzleEditorWindow : EditorWindow
	{
		private const string DEFAULT_FOLDER_PATH = "Assets/Content/Puzzles";
		private const float SIDE_SECTION_WIDTH = 300f;
		public enum EditingMode
		{
			Add,
			Paint,
			Select
		}

		private PuzzleEditorWindowData _windowData = null;

		// Stuff for drawing editor:
		private Vector2 _scrollPosition = Vector2.zero;
		private DrawAreaBorder _starAreaBorder = null;

		private EditorWindowImage _centerImage = null;
		private EditorWindowImage _starHighlighterImage = null;

		private EditorWindowObjectSetterField<PuzzleData> _loadPuzzleDataField = null;
		private EditorWindowObjectSetterField<Texture> _loadReferenceImageField = null;

		private bool _saveModeActive = false;
		private EditingMode _currentMode = EditingMode.Add;
		private Color _currentAddModeColor = Color.white;
		private Color _currentPaintModeColor = Color.white;
		private PuzzleEditorStar _selectedStar = null;
		private PuzzleEditorStar _starBeingDragged = null;
		private Vector2 _draggedStarStartPosition = Vector2.zero;

		private string _switchingScene = null;
		private string _previousScene = null;

		[MenuItem("Window/Puzzle Editor")]
		private static void Init()
		{
			PuzzleEditorWindow window = GetWindow<PuzzleEditorWindow>();
			window.Show();
		}

		private void OnFocus()
		{
			if (_windowData == null)
			{
				_windowData = new PuzzleEditorWindowData();
			}

			if (_centerImage == null)
			{
				Texture centerTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Content/Art/PuzzleToolCenterMark.png");
				_centerImage = new EditorWindowImage(centerTexture, 30f, 30f, _windowData.StarArea.center, Color.white);
			}

			if (_starHighlighterImage == null)
			{
				Texture highlightTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Content/Art/PuzzleEditorStarSelector.png");
				_starHighlighterImage = new EditorWindowImage(highlightTexture, 30f, 30f, _windowData.StarArea.center, Color.white);
				_starHighlighterImage.Visible = false;
			}

			if (_starAreaBorder == null)
			{
				_starAreaBorder = new DrawAreaBorder(_windowData.StarArea, Color.black);
			}

			if (_loadPuzzleDataField == null)
			{
				_loadPuzzleDataField = new EditorWindowObjectSetterField<PuzzleData>("Load  Puzzle", "Load", SIDE_SECTION_WIDTH, LoadInPuzzle);
			}

			if (_loadReferenceImageField == null)
			{
				_loadReferenceImageField = new EditorWindowObjectSetterField<Texture>("Set Reference Image for Puzzle", "Set", SIDE_SECTION_WIDTH, (Texture texture) =>
				{
					_windowData.StarAreaReferenceImage.Texture = texture;
					_windowData.StarAreaReferenceImage.Color = Color.white;
				});
			}
		}

		private void OnGUI()
		{
			if (EditorApplication.isPlaying)
			{
				EditorGUILayout.LabelField("Please exit play mode to edit puzzles with the tool.", EditorStyles.boldLabel);
				return;
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			if (GUILayout.Button("New Puzzle", GUILayout.Width(SIDE_SECTION_WIDTH)))
			{
				if (EditorUtility.DisplayDialog("Are You Sure?", "If you start a new puzzle any unsaved changes in the current puzzle will be discarded. Do you wish to continue?", "Yes", "No"))
				{
					ResetValues();
				}
			}

			DrawSavePuzzleSection();
			if (!_saveModeActive)
			{
				_loadPuzzleDataField.Draw();
				DrawPuzzleDataSection();
				_loadReferenceImageField.Draw();
				DrawModeToggleSection();
				GUILayout.Space(10f);
				DrawActionQueueSection();
				GUILayout.Space(15f);
				DrawTestPuzzleSection();
			}

			// The scroll view stuff does not, as far as I can tell, interact with any of the draw stuff I do for the star field area.
			// So, the only way I can think of to ensure the scroll area encompasses the star field is to just create a buch of gui layout space.
			// Wish there was a way of getting the layout position after all the gui stuff so I can only add exactly how much extra space I need.
			GUILayout.Space(600f);
			GUILayout.BeginHorizontal();
			GUILayout.Space(_windowData.StarArea.max.x + 20f);
			GUILayout.EndHorizontal();

			HandleGUIEvents();

			EditorGUILayout.EndScrollView();

		}

		private void DrawSavePuzzleSection()
		{
			string saveButtonText = _saveModeActive ? "Cancel" : "Save Puzzle";
			if (GUILayout.Button(saveButtonText, GUILayout.Width(SIDE_SECTION_WIDTH)))
			{
				_saveModeActive = !_saveModeActive;
			}

			if (_saveModeActive)
			{
				if (!_windowData.PuzzleDataValidForSaving())
				{
					GUILayout.Label($"In order to save a puzzle, it needs a name, id and at least {PuzzleEditorWindowData.MIN_STARS_IN_PUZZLE} stars.", EditorStyles.wordWrappedLabel, GUILayout.Width(SIDE_SECTION_WIDTH));
				}
				else
				{
					Object testObject = EditorGUILayout.ObjectField("Folder for Puzzle Data", _windowData.FolderForPuzzleFile, typeof(Object), false, GUILayout.Width(SIDE_SECTION_WIDTH));
					if (Directory.Exists(AssetDatabase.GetAssetPath(testObject)))
					{
						_windowData.FolderForPuzzleFile = testObject;
					}

					_windowData.PuzzleFileName = EditorGUILayout.TextField("Filename for Puzzle Data", _windowData.PuzzleFileName, GUILayout.Width(SIDE_SECTION_WIDTH));

					if (_windowData.FolderForPuzzleFile != null && !string.IsNullOrEmpty(_windowData.PuzzleFileName))
					{
						if (GUILayout.Button("Save", GUILayout.Width(SIDE_SECTION_WIDTH)))
						{
							// If that file already exists, load it and overwrite its data. Otherwise create a new PuzzleData and set its data.
							string fullPath = $"{AssetDatabase.GetAssetPath(_windowData.FolderForPuzzleFile)}/{_windowData.PuzzleFileName}.asset";
							if (File.Exists(fullPath))
							{
								if (EditorUtility.DisplayDialog("Overwrite File?", "A puzzle file of that name already exists in that location. Do you wish to overwrite it?", "Yes", "No"))
								{
									SavePuzzleDataFile(fullPath, false);
									_saveModeActive = false;
								}
							}	
							else
							{
								SavePuzzleDataFile(fullPath, true);
								_saveModeActive = false;
							}
						}
					}
				}
			}
		}

		private void SavePuzzleDataFile(string filePath, bool createNew)
		{
			PuzzleData puzzleData = null;
			List<int> starsDeletedForVersion = new List<int>();
			int starsAddedForVersion = 0;
			if (createNew)
			{
				puzzleData = ScriptableObject.CreateInstance<PuzzleData>();
				AssetDatabase.CreateAsset(puzzleData, filePath);
			}
			else
			{
				puzzleData = AssetDatabase.LoadAssetAtPath<PuzzleData>(filePath);
				starsAddedForVersion = _windowData.ActionQueue.GetActionsAsHistoryData(starsDeletedForVersion);
			}

			string imagePath = _windowData.StarAreaReferenceImage.Texture != null ? AssetDatabase.GetAssetPath(_windowData.StarAreaReferenceImage.Texture) : "";
			puzzleData.SetDataFromEditorTool(_windowData.PuzzleId, _windowData.PuzzleName, _windowData.NumPuzzleSpinners, _windowData.Stars, imagePath);
			puzzleData.AddHistoryData(_windowData.NumPuzzleSpinners, starsAddedForVersion, starsDeletedForVersion);
			EditorUtility.SetDirty(puzzleData);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			_windowData.ActionQueue.ClearQueue();
		}

		private void DrawPuzzleDataSection()
		{
			GUILayout.Space(10f);
			_windowData.PuzzleId = EditorGUILayout.TextField("Puzzle Id:", _windowData.PuzzleId, GUILayout.Width(SIDE_SECTION_WIDTH));
			_windowData.PuzzleName = EditorGUILayout.TextField("Puzzle Name:", _windowData.PuzzleName, GUILayout.Width(SIDE_SECTION_WIDTH));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Spinners for Puzzle:", GUILayout.Width(SIDE_SECTION_WIDTH / 2f));
			_windowData.NumPuzzleSpinners = EditorGUILayout.IntSlider(_windowData.NumPuzzleSpinners, 2, 7, GUILayout.Width(SIDE_SECTION_WIDTH / 2f));
			GUILayout.EndHorizontal();
		}

		private void DrawModeToggleSection()
		{
			string[] toggleNames = { "Add Mode", "Paint Mode", "Select Mode" };
			EditingMode mode = (EditingMode)GUILayout.Toolbar((int)_currentMode, toggleNames, GUILayout.Width(SIDE_SECTION_WIDTH));
			// Make sure there is not a selected star if switching from select mode.
			if (_currentMode == EditingMode.Select && mode != EditingMode.Select)
			{
				_selectedStar = null;
			}

			_currentMode = mode;

			if (_currentMode == EditingMode.Add)
			{
				DrawAddModeSection();
			}
			else if (_currentMode == EditingMode.Paint)
			{
				DrawPaintModeSection();
			}
			else if (_currentMode == EditingMode.Select)
			{
				DrawSelectModeSection();
			}
		}

		private void DrawAddModeSection()
		{
			GUILayout.Label("Add Mode: Clicking on the canvas will add a new star whose color is the color selected below.\n\nNote: If you click on a spot too close to an existing star, the star will not be added.", EditorStyles.wordWrappedLabel, GUILayout.Width(SIDE_SECTION_WIDTH));
			_currentAddModeColor = EditorGUILayout.ColorField(_currentAddModeColor, GUILayout.Width(300f));
		}

		private void DrawPaintModeSection()
		{
			GUILayout.Label("Paint Mode: Clicking on on a star on the canvas will change that star's color to the one selected below.", EditorStyles.wordWrappedLabel, GUILayout.Width(SIDE_SECTION_WIDTH));
			_currentPaintModeColor = EditorGUILayout.ColorField(_currentPaintModeColor, GUILayout.Width(300f));
		}

		private void DrawSelectModeSection()
		{
			GUILayout.Label("Select Mode: Clicking on on a star on the canvas will select it or another area to unselect any star. While a star is selected you can drag it to move it or change its colour or press the delete key to remove it.\n\nNote: if you drag a star too close to another, it will return to the spot it was at before when you release the drag.", EditorStyles.wordWrappedLabel, GUILayout.Width(SIDE_SECTION_WIDTH));
			if (_selectedStar != null)
			{
				GUILayout.BeginHorizontal();
				_selectedStar.EndColour = EditorGUILayout.ColorField(_selectedStar.EndColour, GUILayout.Width(SIDE_SECTION_WIDTH / 2f));
				if (GUILayout.Button("Delete", GUILayout.Width(SIDE_SECTION_WIDTH / 2f)))
				{
					_windowData.StarCollisionGrid.PullStarFromGridAtPoint(_selectedStar.EditorPosition);
					int starIndex = _windowData.Stars.IndexOf(_selectedStar);
					if (starIndex > -1)
					{
						AddDeleteStarAction(starIndex, _selectedStar);
					}

					_windowData.Stars.Remove(_selectedStar);
					_selectedStar = null;
				}

				GUILayout.EndHorizontal();
			}
		}

		private void HandleGUIEvents()
		{
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			switch (Event.current.GetTypeForControl(controlID))
			{
				case EventType.MouseDown:
					if (_currentMode == EditingMode.Add)
					{
						if (_windowData.StarArea.Contains(Event.current.mousePosition))
						{
							if (!_windowData.StarCollisionGrid.StarAreaOverlapsStar(Event.current.mousePosition))
							{
								PuzzleEditorStar star = new PuzzleEditorStar(_currentAddModeColor, _windowData.StarArea);
								star.SetPositionsUsingEditorPosiiton(Event.current.mousePosition);
								// At this point, this check should basically never fail, but I think there might be some very small edge cases where it could fail, so it is necessary to still have it.
								if (_windowData.StarCollisionGrid.SetStarToGrid(star))
								{
									_windowData.Stars.Add(star);
									AddAddStarAction();
								}
							}
						}
					}
					else if (_currentMode == EditingMode.Paint)
					{
						if (_windowData.StarArea.Contains(Event.current.mousePosition))
						{
							PuzzleEditorStar star = _windowData.StarCollisionGrid.GetStarAtPoint(Event.current.mousePosition);
							if (star != null)
							{
								Color beforeColor = star.EndColour;
								Color afterColor = _currentPaintModeColor;
								star.EndColour = _currentPaintModeColor;
								int starIndex = _windowData.Stars.IndexOf(star);
								if (starIndex > -1)
								{
									AddColorStarAction(starIndex, beforeColor, afterColor);
								}
							}
						}
					}
					else if (_currentMode == EditingMode.Select)
					{
						if (_windowData.StarArea.Contains(Event.current.mousePosition))
						{
							// If we have a selected star, we want to first check if the click is close (but not necessarily in the star) to be considered still selected.
							bool stillSelected = false;
							if (_selectedStar != null)
							{
								stillSelected = _selectedStar.WithinRangeOfPoint(Event.current.mousePosition, _starHighlighterImage.Width / 2f);
							}

							if (!stillSelected)
							{
								_selectedStar = _windowData.StarCollisionGrid.GetStarAtPoint(Event.current.mousePosition);
							}

							_starBeingDragged = _selectedStar;
							if (_starBeingDragged != null)
							{
								_draggedStarStartPosition = _starBeingDragged.EditorPosition;
								_windowData.StarCollisionGrid.PullStarFromGridAtPoint(_starBeingDragged.EditorPosition);
							}
						}
					}

					Event.current.Use();
					break;

				case EventType.MouseDrag:
					if (_currentMode == EditingMode.Select && _starBeingDragged != null)
					{
						Vector2 dragPos = Event.current.mousePosition;
						dragPos.x = Mathf.Clamp(dragPos.x, _windowData.StarArea.xMin, _windowData.StarArea.xMax);
						dragPos.y = Mathf.Clamp(dragPos.y, _windowData.StarArea.yMin, _windowData.StarArea.yMax);

						_starBeingDragged.SetPositionsUsingEditorPosiiton(dragPos);
					}

					Event.current.Use();
					break;

				case EventType.MouseUp:
					if (_starBeingDragged != null)
					{
						bool successfullySetStar = false;
						if (!_windowData.StarCollisionGrid.StarAreaOverlapsStar(_starBeingDragged.EditorPosition))
						{
							if (_windowData.StarCollisionGrid.SetStarToGrid(_starBeingDragged))
							{
								successfullySetStar = true;
								int starIndex = _windowData.Stars.IndexOf(_starBeingDragged);
								if (starIndex > -1 && _draggedStarStartPosition != _starBeingDragged.EditorPosition)
								{
									AddMoveStarAction(starIndex, _draggedStarStartPosition, _starBeingDragged.EditorPosition);
								}
							}
						}

						if (!successfullySetStar)
						{
							_starBeingDragged.SetPositionsUsingEditorPosiiton(_draggedStarStartPosition);
							_windowData.StarCollisionGrid.SetStarToGrid(_starBeingDragged);
						}
					}

					_starBeingDragged = null;
					_draggedStarStartPosition = Vector2.zero;
					Event.current.Use();
					break;

				case EventType.KeyDown:
					if (Event.current.keyCode == KeyCode.Delete)
					{
						if (_currentMode == EditingMode.Select && _selectedStar != null)
						{
							_windowData.StarCollisionGrid.PullStarFromGridAtPoint(_selectedStar.EditorPosition);
							int starIndex = _windowData.Stars.IndexOf(_selectedStar);
							if (starIndex > -1)
							{
								AddDeleteStarAction(starIndex, _selectedStar);
							}

							_windowData.Stars.Remove(_selectedStar);
							_selectedStar = null;
						}
					}

					Event.current.Use();
					break;

				case EventType.Repaint:
					DrawStarField();
					break;
			}
		}

		private void DrawActionQueueSection()
		{
			EditorGUILayout.LabelField("Undo/Redo Star Changing Actions.", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();

			if (!_windowData.ActionQueue.UndoActionsAvailable)
			{
				GUI.enabled = false;
			}

			if (GUILayout.Button("Undo", GUILayout.Width(SIDE_SECTION_WIDTH / 2f)))
			{
				_selectedStar = null;
				_windowData.ActionQueue.UndoAction();
			}

			GUI.enabled = true;

			if (!_windowData.ActionQueue.RedoActionsAvailable)
			{
				GUI.enabled = false;
			}

			if (GUILayout.Button("Redo", GUILayout.Width(SIDE_SECTION_WIDTH / 2f)))
			{
				_selectedStar = null;
				_windowData.ActionQueue.RedoAction();
			}

			GUI.enabled = true;

			GUILayout.EndHorizontal();
		}

		private void DrawTestPuzzleSection()
		{
			EditorGUILayout.LabelField("Puzzle Testing:", EditorStyles.boldLabel);
			if (!_windowData.PuzzleDataValidForTesting())
			{
				GUILayout.Label($"The puzzle needs to have a name and at least {PuzzleEditorWindowData.MIN_STARS_IN_PUZZLE} stars in order to be tested.", EditorStyles.wordWrappedLabel, GUILayout.Width(SIDE_SECTION_WIDTH));
			}
			else
			{
				if (_switchingScene == EditorSceneManager.GetActiveScene().path)
				{
					_windowData.PutDataIntoEditorPrefs();
					EditorApplication.isPlaying = true;
					_switchingScene = null;
				}

				if (GUILayout.Button("Test Puzzle", GUILayout.Width(SIDE_SECTION_WIDTH)))
				{
					_previousScene = EditorSceneManager.GetActiveScene().path;
					_switchingScene = "Assets/Scripts/Editor/Puzzle Tool/PuzzleTestScene.unity";
					EditorSceneManager.OpenScene(_switchingScene);
				}
			}
		}

		private void Update()
		{
			if (!EditorApplication.isPlaying)
			{
				if (string.IsNullOrEmpty(_switchingScene) && !string.IsNullOrEmpty(_previousScene))
				{
					EditorSceneManager.OpenScene(_previousScene);
					_previousScene = null;
					_windowData.GetDataFromEditorPrefs();
				}
			}
		}

		private void DrawStarField()
		{
			_windowData.StarAreaReferenceImage.Draw();
			_starAreaBorder.Draw();
			_centerImage.Draw();

			if (_selectedStar != null)
			{
				_starHighlighterImage.Visible = true;
				_starHighlighterImage.Position = _selectedStar.EditorPosition;
			}
			else
			{
				_starHighlighterImage.Visible = false;
			}

			_starHighlighterImage.Draw();

			foreach (PuzzleEditorStar star in _windowData.Stars)
			{
				star.Draw();
			}
		}

		private void LoadInPuzzle(PuzzleData dataToLoad)
		{
			if (dataToLoad != null)
			{
				if (EditorUtility.DisplayDialog("Load Puzzle?", "If you load a puzzle, any unsaved changes you have in the current puzzle will be lost. Do you want to continue?", "Yes", "No"))
				{
					ResetValues();

					_windowData.FolderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(AssetDatabase.GetAssetPath(dataToLoad)));
					_windowData.PuzzleFileName = dataToLoad.name;
					_windowData.PuzzleId = dataToLoad.PuzzleUniqueId;
					_windowData.PuzzleName = dataToLoad.PuzzleName;
					if (!string.IsNullOrEmpty(dataToLoad.PuzzleImageReferencePath))
					{
						if (File.Exists(dataToLoad.PuzzleImageReferencePath))
						{
							_windowData.StarAreaReferenceImage.Texture = AssetDatabase.LoadAssetAtPath<Texture>(dataToLoad.PuzzleImageReferencePath);
							_windowData.StarAreaReferenceImage.Color = Color.white;
						}
					}

					_windowData.NumPuzzleSpinners = dataToLoad.NumSpinners;
					foreach (PuzzleData.StarData starData in dataToLoad.StarDatas)
					{
						PuzzleEditorStar star = new PuzzleEditorStar(starData.FinalColor, _windowData.StarArea);
						star.SetPositionUsingGamePosition(starData.Position);
						_windowData.StarCollisionGrid.SetStarToGrid(star);
						_windowData.Stars.Add(star);
					}
				}
			}
		}

		private void ResetValues()
		{
			_windowData.ResetValues();

			_saveModeActive = false;
			_selectedStar = null;
		}

		private void AddAddStarAction()
		{
			PuzzleEditorStar starAdded = _windowData.Stars[_windowData.Stars.Count - 1];
			PuzzleAddStarAction action = new PuzzleAddStarAction(_windowData.Stars, _windowData.StarCollisionGrid, starAdded);
			_windowData.ActionQueue.AddAction(action);
		}

		private void AddDeleteStarAction(int starDeletedIndex, PuzzleEditorStar deletedStar)
		{
			PuzzleDeleteStarAction action = new PuzzleDeleteStarAction(_windowData.Stars, _windowData.StarCollisionGrid, deletedStar, starDeletedIndex);
			_windowData.ActionQueue.AddAction(action);
		}

		private void AddMoveStarAction(int starChangedIndex, Vector2 beforePosition, Vector2 afterPosition)
		{
			PuzzleMoveStarAction action = new PuzzleMoveStarAction(_windowData.Stars, _windowData.StarCollisionGrid, starChangedIndex, beforePosition, afterPosition);
			_windowData.ActionQueue.AddAction(action);
		}

		private void AddColorStarAction(int starChangedIndex, Color beforeColor, Color afterColor)
		{
			PuzzleColorStarAction action = new PuzzleColorStarAction(_windowData.Stars, starChangedIndex, beforeColor, afterColor);
			_windowData.ActionQueue.AddAction(action);
		}
	}
}

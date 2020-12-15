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
		public const string DATA_BEING_EDITED_PREFS_KEY = "puzzle_data_being_edited";
		public const string PUZZLE_ID_KEY = "puzzle_id";
		public const string PUZZLE_NAME_KEY = "puzzle_name";
		public const string PUZZLE_NUM_SPINNERS_KEY = "puzzle_num_spinners";
		public const string PUZZLE_STAR_POSITION_X_KEY = "pos_x";
		public const string PUZZLE_STAR_POSITION_Y_KEY = "pos_y";
		public const string PUZZLE_STAR_COLOR_R_KEY = "color_r";
		public const string PUZZLE_STAR_COLOR_G_KEY = "color_g";
		public const string PUZZLE_STAR_COLOR_B_KEY = "color_b";
		public const string PUZZLE_STARS_KEY = "stars";
		private const string PUZZLE_IMAGE_REF_KEY = "puzzle_image_ref";
		private const string PUZZLE_FOLDER_KEY = "puzzle_folder";
		private const string PUZZLE_FILE_KEY = "puzzle_file";
		private const string PUZZLE_ACTION_QUEUE_KEY = "action_queue";

		private const int MIN_STARS_IN_PUZZLE = 15;
		private const string DEFAULT_FOLDER_PATH = "Assets/Content/Puzzles";
		private const float SIDE_SECTION_WIDTH = 300f;
		public enum EditingMode
		{
			Add,
			Paint,
			Select
		}

		// File data section:
		private Object _defaultFolderForPuzzleFile = null;
		private Object _folderForPuzzleFile = null;
		private string _puzzleFileName = null;
		private string _puzzleId;
		private string _puzzleName;
		private int _numPuzzleSpinners;
		private List<PuzzleEditorStar> _stars = new List<PuzzleEditorStar>();

		// Stuff for drawing editor:
		private Vector2 _scrollPosition = Vector2.zero;
		private Rect _starArea = new Rect(320, 15, 650, 650);
		private DrawAreaBorder _starAreaBorder = null;
		private StarCollisionGrid _starCollisionGrid = null;

		private EditorWindowImage _starAreaReferenceImage = null;
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
		private PuzzleToolActionQueue _actionQueue = new PuzzleToolActionQueue();

		[MenuItem("Window/Puzzle Editor")]
		private static void Init()
		{
			PuzzleEditorWindow window = GetWindow<PuzzleEditorWindow>();
			window.Show();
		}

		private void OnFocus()
		{
			if (_defaultFolderForPuzzleFile == null)
			{
				if (Directory.Exists(DEFAULT_FOLDER_PATH))
				{
					_defaultFolderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(DEFAULT_FOLDER_PATH);
				}
				else
				{
					// Default to the main Assets folder just in case, which will for sure exist.
					_defaultFolderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>("Assets");
				}
			}

			if (_starCollisionGrid == null)
			{
				_starCollisionGrid = new StarCollisionGrid(_starArea);
			}

			if (_starAreaReferenceImage == null)
			{
				_starAreaReferenceImage = new EditorWindowImage(null, _starArea.width, _starArea.height, _starArea.center, Color.black);
			}

			if (_centerImage == null)
			{
				Texture centerTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Content/Art/PuzzleToolCenterMark.png");
				_centerImage = new EditorWindowImage(centerTexture, 30f, 30f, _starArea.center, Color.white);
			}

			if (_starHighlighterImage == null)
			{
				Texture highlightTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Content/Art/PuzzleEditorStarSelector.png");
				_starHighlighterImage = new EditorWindowImage(highlightTexture, 30f, 30f, _starArea.center, Color.white);
				_starHighlighterImage.Visible = false;
			}

			if (_starAreaBorder == null)
			{
				_starAreaBorder = new DrawAreaBorder(_starArea, Color.black);
			}

			if (_loadPuzzleDataField == null)
			{
				_loadPuzzleDataField = new EditorWindowObjectSetterField<PuzzleData>("Load  Puzzle", "Load", SIDE_SECTION_WIDTH, LoadInPuzzle);
			}

			if (_loadReferenceImageField == null)
			{
				_loadReferenceImageField = new EditorWindowObjectSetterField<Texture>("Set Reference Image for Puzzle", "Set", SIDE_SECTION_WIDTH, (Texture texture) =>
				{
					_starAreaReferenceImage.Texture = texture;
					_starAreaReferenceImage.Color = Color.white;
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
			GUILayout.Space(_starArea.max.x + 20f);
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
				if (!PuzzleDataValidForSaving())
				{
					GUILayout.Label($"In order to save a puzzle, it needs a name, id and at\nleast {MIN_STARS_IN_PUZZLE} stars.", GUILayout.Width(SIDE_SECTION_WIDTH));
				}
				else
				{
					Object testObject = EditorGUILayout.ObjectField("Folder for Puzzle Data", _folderForPuzzleFile, typeof(Object), false, GUILayout.Width(SIDE_SECTION_WIDTH));
					if (Directory.Exists(AssetDatabase.GetAssetPath(testObject)))
					{
						_folderForPuzzleFile = testObject;
					}

					_puzzleFileName = EditorGUILayout.TextField("Filename for Puzzle Data", _puzzleFileName, GUILayout.Width(SIDE_SECTION_WIDTH));

					if (_folderForPuzzleFile != null && !string.IsNullOrEmpty(_puzzleFileName))
					{
						if (GUILayout.Button("Save", GUILayout.Width(SIDE_SECTION_WIDTH)))
						{
							// If that file already exists, load it and overwrite its data. Otherwise create a new PuzzleData and set its data.
							string fullPath = $"{AssetDatabase.GetAssetPath(_folderForPuzzleFile)}/{_puzzleFileName}.asset";
							string imagePath = "";
							if (_starAreaReferenceImage.Texture != null)
							{
								imagePath = AssetDatabase.GetAssetPath(_starAreaReferenceImage.Texture);
							}
							
							if (File.Exists(fullPath))
							{
								if (EditorUtility.DisplayDialog("Overwrite File?", "A puzzle file of that name already exists in that location. Do you wish to overwrite it?", "Yes", "No"))
								{
									PuzzleData puzzleData = AssetDatabase.LoadAssetAtPath<PuzzleData>(fullPath);
									puzzleData.SetDataFromEditorTool(_puzzleId, _puzzleName, _numPuzzleSpinners, _stars, imagePath);
									List<int> starsDeletedForVersion = new List<int>();
									int starsAddedForVersion = _actionQueue.GetActionsAsHistoryData(starsDeletedForVersion);
									puzzleData.AddHistoryData(_numPuzzleSpinners, starsAddedForVersion, starsDeletedForVersion);
									EditorUtility.SetDirty(puzzleData);
									AssetDatabase.SaveAssets();
									AssetDatabase.Refresh();
									_actionQueue.ClearQueue();
									_saveModeActive = false;
								}
							}	
							else
							{
								PuzzleData puzzleData = ScriptableObject.CreateInstance<PuzzleData>();
								puzzleData.SetDataFromEditorTool(_puzzleId, _puzzleName, _numPuzzleSpinners, _stars, imagePath);
								puzzleData.AddHistoryData(_numPuzzleSpinners, 0, new List<int>());
								EditorUtility.SetDirty(puzzleData);
								AssetDatabase.CreateAsset(puzzleData, fullPath);
								AssetDatabase.SaveAssets();
								AssetDatabase.Refresh();
								_actionQueue.ClearQueue();
								_saveModeActive = false;
							}
						}
					}
				}
			}
		}

		private void DrawPuzzleDataSection()
		{
			GUILayout.Space(10f);
			_puzzleId = EditorGUILayout.TextField("Puzzle Id:", _puzzleId, GUILayout.Width(SIDE_SECTION_WIDTH));
			_puzzleName = EditorGUILayout.TextField("Puzzle Name:", _puzzleName, GUILayout.Width(SIDE_SECTION_WIDTH));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Spinners for Puzzle:", GUILayout.Width(SIDE_SECTION_WIDTH / 2f));
			_numPuzzleSpinners = EditorGUILayout.IntSlider(_numPuzzleSpinners, 2, 7, GUILayout.Width(SIDE_SECTION_WIDTH / 2f));
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
			GUILayout.Label("Add Mode: Clicking on the canvas will add a new\nstar whose color is the color selected below.", GUILayout.Width(SIDE_SECTION_WIDTH));
			GUILayout.Label("Note: If you click on a spot too close to an\nexisting star, the star will not be added.", GUILayout.Width(SIDE_SECTION_WIDTH));
			_currentAddModeColor = EditorGUILayout.ColorField(_currentAddModeColor, GUILayout.Width(300f));
		}

		private void DrawPaintModeSection()
		{
			GUILayout.Label("Paint Mode: Clicking on on a star on the canvas\nwill change that star's color to the one selected\nbelow.", GUILayout.Width(SIDE_SECTION_WIDTH));
			_currentPaintModeColor = EditorGUILayout.ColorField(_currentPaintModeColor, GUILayout.Width(300f));
		}

		private void DrawSelectModeSection()
		{
			GUILayout.Label("Select Mode: Clicking on on a star on the canvas\nwill select it or another area to unselect any star.\nWhile a star is selected you can drag it to move it or\nchange its colour or press the delete key to remove it.", GUILayout.Width(SIDE_SECTION_WIDTH));
			GUILayout.Label("Note: if you drag a star too close to another, it\nwill return to the spot it was at before when you\nrelease the drag.", GUILayout.Width(SIDE_SECTION_WIDTH));
			if (_selectedStar != null)
			{
				GUILayout.BeginHorizontal();
				_selectedStar.EndColour = EditorGUILayout.ColorField(_selectedStar.EndColour, GUILayout.Width(SIDE_SECTION_WIDTH / 2f));
				if (GUILayout.Button("Delete", GUILayout.Width(SIDE_SECTION_WIDTH / 2f)))
				{
					_starCollisionGrid.PullStarFromGridAtPoint(_selectedStar.EditorPosition);
					int starIndex = _stars.IndexOf(_selectedStar);
					if (starIndex > -1)
					{
						AddDeleteStarAction(starIndex, _selectedStar);
					}

					_stars.Remove(_selectedStar);
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
						if (_starArea.Contains(Event.current.mousePosition))
						{
							if (!_starCollisionGrid.StarAreaOverlapsStar(Event.current.mousePosition))
							{
								PuzzleEditorStar star = new PuzzleEditorStar(_currentAddModeColor, _starArea);
								star.SetPositionsUsingEditorPosiiton(Event.current.mousePosition);
								// At this point, this check should basically never fail, but I think there might be some very small edge cases where it could fail, so it is necessary to still have it.
								if (_starCollisionGrid.SetStarToGrid(star))
								{
									_stars.Add(star);
									AddAddStarAction();
								}
							}
						}
					}
					else if (_currentMode == EditingMode.Paint)
					{
						if (_starArea.Contains(Event.current.mousePosition))
						{
							PuzzleEditorStar star = _starCollisionGrid.GetStarAtPoint(Event.current.mousePosition);//GetClickedOnStar(Event.current.mousePosition);
							if (star != null)
							{
								Color beforeColor = star.EndColour;
								Color afterColor = _currentPaintModeColor;
								star.EndColour = _currentPaintModeColor;
								int starIndex = _stars.IndexOf(star);
								if (starIndex > -1)
								{
									AddColorStarAction(starIndex, beforeColor, afterColor);
								}
							}
						}
					}
					else if (_currentMode == EditingMode.Select)
					{
						if (_starArea.Contains(Event.current.mousePosition))
						{
							// If we have a selected star, we want to first check if the click is close (but not necessarily in the star) to be considered still selected.
							bool stillSelected = false;
							if (_selectedStar != null)
							{
								stillSelected = _selectedStar.WithinRangeOfPoint(Event.current.mousePosition, _starHighlighterImage.Width / 2f);
							}

							if (!stillSelected)
							{
								_selectedStar = _starCollisionGrid.GetStarAtPoint(Event.current.mousePosition);
							}

							_starBeingDragged = _selectedStar;
							if (_starBeingDragged != null)
							{
								_draggedStarStartPosition = _starBeingDragged.EditorPosition;
								_starCollisionGrid.PullStarFromGridAtPoint(_starBeingDragged.EditorPosition);
							}
						}
					}

					Event.current.Use();
					break;

				case EventType.MouseDrag:
					if (_currentMode == EditingMode.Select && _starBeingDragged != null)
					{
						Vector2 dragPos = Event.current.mousePosition;
						dragPos.x = Mathf.Clamp(dragPos.x, _starArea.xMin, _starArea.xMax);
						dragPos.y = Mathf.Clamp(dragPos.y, _starArea.yMin, _starArea.yMax);

						_starBeingDragged.SetPositionsUsingEditorPosiiton(dragPos);
					}

					Event.current.Use();
					break;

				case EventType.MouseUp:
					if (_starBeingDragged != null)
					{
						bool successfullySetStar = false;
						if (!_starCollisionGrid.StarAreaOverlapsStar(_starBeingDragged.EditorPosition))
						{
							if (_starCollisionGrid.SetStarToGrid(_starBeingDragged))
							{
								successfullySetStar = true;
								int starIndex = _stars.IndexOf(_starBeingDragged);
								if (starIndex > -1 && _draggedStarStartPosition != _starBeingDragged.EditorPosition)
								{
									AddMoveStarAction(starIndex, _draggedStarStartPosition, _starBeingDragged.EditorPosition);
								}
							}
						}

						if (!successfullySetStar)
						{
							_starBeingDragged.SetPositionsUsingEditorPosiiton(_draggedStarStartPosition);
							_starCollisionGrid.SetStarToGrid(_starBeingDragged);
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
							_starCollisionGrid.PullStarFromGridAtPoint(_selectedStar.EditorPosition);
							int starIndex = _stars.IndexOf(_selectedStar);
							if (starIndex > -1)
							{
								AddDeleteStarAction(starIndex, _selectedStar);
							}

							_stars.Remove(_selectedStar);
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

			if (!_actionQueue.UndoActionsAvailable)
			{
				GUI.enabled = false;
			}

			if (GUILayout.Button("Undo", GUILayout.Width(SIDE_SECTION_WIDTH / 2f)))
			{
				_selectedStar = null;
				_actionQueue.UndoAction();
			}

			GUI.enabled = true;

			if (!_actionQueue.RedoActionsAvailable)
			{
				GUI.enabled = false;
			}

			if (GUILayout.Button("Redo", GUILayout.Width(SIDE_SECTION_WIDTH / 2f)))
			{
				_selectedStar = null;
				_actionQueue.RedoAction();
			}

			GUI.enabled = true;

			GUILayout.EndHorizontal();
		}

		private void DrawTestPuzzleSection()
		{
			EditorGUILayout.LabelField("Puzzle Testing:", EditorStyles.boldLabel);
			if (!PuzzleDataValidForTesting())
			{
				GUILayout.Label($"The puzzle needs to have a name and at least\n{MIN_STARS_IN_PUZZLE} stars in order to be tested.", GUILayout.Width(SIDE_SECTION_WIDTH));
			}
			else
			{
				if (_switchingScene == EditorSceneManager.GetActiveScene().path)
				{
					PutDataIntoEditorPrefs();
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
					GetDataFromEditorPrefs();
				}
			}
		}

		private void DrawStarField()
		{
			_starAreaReferenceImage.Draw();
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

			foreach (PuzzleEditorStar star in _stars)
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

					_folderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(AssetDatabase.GetAssetPath(dataToLoad)));
					_puzzleFileName = dataToLoad.name;
					_puzzleId = dataToLoad.PuzzleUniqueId;
					_puzzleName = dataToLoad.PuzzleName;
					if (!string.IsNullOrEmpty(dataToLoad.PuzzleImageReferencePath))
					{
						if (File.Exists(dataToLoad.PuzzleImageReferencePath))
						{
							_starAreaReferenceImage.Texture = AssetDatabase.LoadAssetAtPath<Texture>(dataToLoad.PuzzleImageReferencePath);
							_starAreaReferenceImage.Color = Color.white;
						}
					}

					_numPuzzleSpinners = dataToLoad.NumSpinners;
					foreach (PuzzleData.StarData starData in dataToLoad.StarDatas)
					{
						PuzzleEditorStar star = new PuzzleEditorStar(starData.FinalColor, _starArea);
						star.SetPositionUsingGamePosition(starData.Position);
						_starCollisionGrid.SetStarToGrid(star);
						_stars.Add(star);
					}
				}
			}
		}

		private void ResetValues()
		{
			_actionQueue.ClearQueue();
			_folderForPuzzleFile = _defaultFolderForPuzzleFile;
			_puzzleFileName = "NewPuzzle";
			_puzzleId = "";
			_puzzleName = "";
			_numPuzzleSpinners = 4;
			_saveModeActive = false;
			_starAreaReferenceImage.Texture = null;
			_starAreaReferenceImage.Color = Color.black;
			_selectedStar = null;
			_starCollisionGrid.ClearGrid();
			_stars.Clear();
		}

		private void PutDataIntoEditorPrefs()
		{
			JSONNode node = new JSONObject();
			node[PUZZLE_ID_KEY] = _puzzleId;
			node[PUZZLE_NAME_KEY] = _puzzleName;
			node[PUZZLE_NUM_SPINNERS_KEY] = _numPuzzleSpinners;
			node[PUZZLE_IMAGE_REF_KEY] = _starAreaReferenceImage.Texture == null ? "null" : AssetDatabase.GetAssetPath(_starAreaReferenceImage.Texture);
			node[PUZZLE_FOLDER_KEY] = AssetDatabase.GetAssetPath(_folderForPuzzleFile);
			node[PUZZLE_FILE_KEY] = _puzzleFileName;

			foreach (PuzzleEditorStar star in _stars)
			{
				JSONObject starData = new JSONObject();
				starData[PUZZLE_STAR_COLOR_R_KEY] = star.EndColour.r;
				starData[PUZZLE_STAR_COLOR_G_KEY] = star.EndColour.g;
				starData[PUZZLE_STAR_COLOR_B_KEY] = star.EndColour.b;
				starData[PUZZLE_STAR_POSITION_X_KEY] = star.GamePosition.x;
				starData[PUZZLE_STAR_POSITION_Y_KEY] = star.GamePosition.y;

				node[PUZZLE_STARS_KEY][-1] = starData;
			}

			node[PUZZLE_ACTION_QUEUE_KEY] = _actionQueue.GetQueueDataAsNode();

			StringBuilder builder = new StringBuilder();
			node.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
			EditorPrefs.SetString(DATA_BEING_EDITED_PREFS_KEY, builder.ToString());
		}

		private void GetDataFromEditorPrefs()
		{
			if (EditorPrefs.HasKey(DATA_BEING_EDITED_PREFS_KEY))
			{
				string json = EditorPrefs.GetString(DATA_BEING_EDITED_PREFS_KEY);
				JSONNode node = JSONNode.Parse(json);
				_puzzleId = node[PUZZLE_ID_KEY].Value;
				_puzzleName = node[PUZZLE_NAME_KEY].Value;
				_numPuzzleSpinners = node[PUZZLE_NUM_SPINNERS_KEY].AsInt;
				string imagePath = node[PUZZLE_IMAGE_REF_KEY].Value;
				if (!string.IsNullOrEmpty(imagePath) && imagePath != "null" && File.Exists(imagePath))
				{
					_starAreaReferenceImage.Texture = AssetDatabase.LoadAssetAtPath<Texture>(imagePath);
					_starAreaReferenceImage.Color = Color.white;
				}

				_folderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(node[PUZZLE_FOLDER_KEY].Value));
				_puzzleFileName = node[PUZZLE_FILE_KEY].Value;

				JSONArray starArray = node[PUZZLE_STARS_KEY].AsArray;
				for (int i = 0; i < starArray.Count; i++)
				{
					JSONNode starNode = starArray[i];
					float colR = starNode[PUZZLE_STAR_COLOR_R_KEY];
					float colG = starNode[PUZZLE_STAR_COLOR_G_KEY];
					float colB = starNode[PUZZLE_STAR_COLOR_B_KEY];
					float posX = starNode[PUZZLE_STAR_POSITION_X_KEY];
					float posY = starNode[PUZZLE_STAR_POSITION_Y_KEY];

					Color starColor = new Color(colR, colG, colB);
					Vector2 pos = new Vector2(posX, posY);

					PuzzleEditorStar star = new PuzzleEditorStar(starColor, _starArea);
					star.SetPositionUsingGamePosition(pos);
					if (_starCollisionGrid.SetStarToGrid(star))
					{
						_stars.Add(star);
					}
				}

				_actionQueue.SetDataFromNode(node[PUZZLE_ACTION_QUEUE_KEY], _stars, _starCollisionGrid);

				EditorPrefs.DeleteKey(DATA_BEING_EDITED_PREFS_KEY);
			}
		}

		private bool PuzzleDataValidForSaving()
		{
			bool validNumStars = _stars.Count >= MIN_STARS_IN_PUZZLE;
			bool validPuzzleId = !string.IsNullOrEmpty(_puzzleId);
			bool validPuzzleName = !string.IsNullOrEmpty(_puzzleName);

			return validNumStars && validPuzzleId && validPuzzleName;
		}

		private bool PuzzleDataValidForTesting()
		{
			bool validNumStars = _stars.Count >= MIN_STARS_IN_PUZZLE;
			bool validPuzzleName = !string.IsNullOrEmpty(_puzzleName);

			return validNumStars && validPuzzleName;
		}

		private void AddAddStarAction()
		{
			PuzzleEditorStar starAdded = _stars[_stars.Count - 1];
			PuzzleAddStarAction action = new PuzzleAddStarAction(_stars, _starCollisionGrid, starAdded);
			_actionQueue.AddAction(action);
		}

		private void AddDeleteStarAction(int starDeletedIndex, PuzzleEditorStar deletedStar)
		{
			PuzzleDeleteStarAction action = new PuzzleDeleteStarAction(_stars, _starCollisionGrid, deletedStar, starDeletedIndex);
			_actionQueue.AddAction(action);
		}

		private void AddMoveStarAction(int starChangedIndex, Vector2 beforePosition, Vector2 afterPosition)
		{
			PuzzleMoveStarAction action = new PuzzleMoveStarAction(_stars, _starCollisionGrid, starChangedIndex, beforePosition, afterPosition);
			_actionQueue.AddAction(action);
		}

		private void AddColorStarAction(int starChangedIndex, Color beforeColor, Color afterColor)
		{
			PuzzleColorStarAction action = new PuzzleColorStarAction(_stars, starChangedIndex, beforeColor, afterColor);
			_actionQueue.AddAction(action);
		}
	}
}

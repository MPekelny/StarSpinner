using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorWindowStuff
{
	public class PuzzleEditorWindow : EditorWindow
	{
		private const int MIN_STARS_IN_PUZZLE = 15;
		private const string DEFAULT_FOLDER_PATH = "Assets/Content/Puzzles";
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
		public int _numPuzzleSpinners;
		private List<PuzzleEditorStar> _stars = new List<PuzzleEditorStar>();

		// Stuff for drawing editor:
		private Vector2 _scrollPosition = Vector2.zero;
		private Rect _starArea = new Rect(320, 15, 650, 650);
		private DrawAreaBorder _starAreaBorder = null;

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

			if (_starAreaReferenceImage == null)
			{
				_starAreaReferenceImage = new EditorWindowImage(null, _starArea.width, _starArea.height, _starArea.center, Color.black);
			}

			if (_centerImage == null)
			{
				Texture centerTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Content/Art/PuzzleToolCenterMark.png");
				_centerImage = new EditorWindowImage(centerTexture, 50f, 50f, _starArea.center, Color.white);
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
				_loadPuzzleDataField = new EditorWindowObjectSetterField<PuzzleData>("Load  Puzzle", "Load", 300f, LoadInPuzzle);
			}

			if (_loadReferenceImageField == null)
			{
				_loadReferenceImageField = new EditorWindowObjectSetterField<Texture>("Set Reference Image for Puzzle", "Set", 300f, (Texture texture) =>
				{
					_starAreaReferenceImage.Texture = texture;
					_starAreaReferenceImage.Color = Color.white;
				});
			}
		}

		private void OnGUI()
		{
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			if (GUILayout.Button("New Puzzle", GUILayout.Width(300f)))
			{
				ResetValues();
			}

			DrawSavePuzzleSection();
			if (!_saveModeActive)
			{
				_loadPuzzleDataField.Draw();
				DrawPuzzleDataSection();
				_loadReferenceImageField.Draw();
				DrawModeToggleSection();
			}

			// The scroll view stuff does not, as far as I can tell, interact with any of the draw stuff I do for the star field area.
			// So, the only way I can think of to ensure the scroll area encompasses the star field is to just create a buch of gui layout space.
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
			if (GUILayout.Button(saveButtonText, GUILayout.Width(300f)))
			{
				_saveModeActive = !_saveModeActive;
			}

			if (_saveModeActive)
			{
				if (!PuzzleDataValidForSaving())
				{
					GUILayout.Label($"In order to save a puzzle, it needs a name, id and at\nleast {MIN_STARS_IN_PUZZLE} stars.", GUILayout.Width(300f));
				}
				else
				{
					Object testObject = EditorGUILayout.ObjectField("Folder for Puzzle Data", _folderForPuzzleFile, typeof(Object), false, GUILayout.Width(300f));
					if (Directory.Exists(AssetDatabase.GetAssetPath(testObject)))
					{
						_folderForPuzzleFile = testObject;
					}

					_puzzleFileName = EditorGUILayout.TextField("Filename for Puzzle Data", _puzzleFileName, GUILayout.Width(300f));

					if (_folderForPuzzleFile != null && !string.IsNullOrEmpty(_puzzleFileName))
					{
						if (GUILayout.Button("Save", GUILayout.Width(300f)))
						{
							// If that file already exists, load it and overwrite its data. Otherwise create a new PuzzleData and set its data.
							string fullPath = $"{AssetDatabase.GetAssetPath(testObject)}/{_puzzleFileName}.asset";
							if (File.Exists(fullPath))
							{
								PuzzleData puzzleData = AssetDatabase.LoadAssetAtPath<PuzzleData>(fullPath);
								puzzleData.SetDataFromEditorTool(_puzzleId, _puzzleName, _numPuzzleSpinners, _stars);
							}	
							else
							{
								PuzzleData puzzleData = ScriptableObject.CreateInstance<PuzzleData>();
								puzzleData.SetDataFromEditorTool(_puzzleId, _puzzleName, _numPuzzleSpinners, _stars);
								AssetDatabase.CreateAsset(puzzleData, fullPath);
							}

							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();
							_saveModeActive = false;
						}
					}
				}
			}
		}

		private void DrawPuzzleDataSection()
		{
			GUILayout.Space(10f);
			_puzzleId = EditorGUILayout.TextField("Puzzle Id:", _puzzleId, GUILayout.Width(300f));
			_puzzleName = EditorGUILayout.TextField("Puzzle Name:", _puzzleName, GUILayout.Width(300f));
			GUILayout.BeginHorizontal();
			GUILayout.Label("Spinners for Puzzle:", GUILayout.Width(150f));
			_numPuzzleSpinners = EditorGUILayout.IntSlider(_numPuzzleSpinners, 2, 7, GUILayout.Width(150f));
			GUILayout.EndHorizontal();
		}

		private void DrawModeToggleSection()
		{
			string[] toggleNames = { "Add Mode", "Paint Mode", "Select Mode" };
			EditingMode mode = (EditingMode)GUILayout.Toolbar((int)_currentMode, toggleNames, GUILayout.Width(300f));
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
			GUILayout.Label("Add Mode: Clicking on the canvas will add a new\nstar whose color is the color selected below.", GUILayout.Width(300f));
			_currentAddModeColor = EditorGUILayout.ColorField(_currentAddModeColor, GUILayout.Width(300f));
		}

		private void DrawPaintModeSection()
		{
			GUILayout.Label("Paint Mode: Clicking on on a star on the canvas\nwill change that star's color to the one selected\nbelow.", GUILayout.Width(300f));
			_currentPaintModeColor = EditorGUILayout.ColorField(_currentPaintModeColor, GUILayout.Width(300f));
		}

		private void DrawSelectModeSection()
		{
			GUILayout.Label("Select Mode: Clicking on on a star on the canvas\nwill select it or another area to unselect any star.\nWhile a star is selected you can drag it to move it or\nchange its colour or press the delete key to remove it.", GUILayout.Width(310f));
			if (_selectedStar != null)
			{
				GUILayout.BeginHorizontal();
				_selectedStar.EndColour = EditorGUILayout.ColorField(_selectedStar.EndColour, GUILayout.Width(150f));
				if (GUILayout.Button("Delete", GUILayout.Width(150f)))
				{
					_stars.Remove(_selectedStar);
					_selectedStar = null;
					_starHighlighterImage.Visible = false;
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
							PuzzleEditorStar star = new PuzzleEditorStar(_currentAddModeColor, _starArea);
							star.SetPositionsUsingEditorPosiiton(Event.current.mousePosition);
							_stars.Add(star);
						}
					}
					else if (_currentMode == EditingMode.Paint)
					{
						if (_starArea.Contains(Event.current.mousePosition))
						{
							PuzzleEditorStar star = GetClickedOnStar(Event.current.mousePosition);
							if (star != null)
							{
								star.EndColour = _currentPaintModeColor;
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
								_selectedStar = GetClickedOnStar(Event.current.mousePosition);
							}

							_starBeingDragged = _selectedStar;
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
					_starBeingDragged = null;
					Event.current.Use();
					break;

				case EventType.KeyDown:
					if (Event.current.keyCode == KeyCode.Delete)
					{
						if (_currentMode == EditingMode.Select && _selectedStar != null)
						{
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

		private PuzzleEditorStar GetClickedOnStar(Vector2 clickPos)
		{
			// For the moment, just look at all the stars, but will want to use something like a quad tree to make star lookup more efficient.
			foreach (PuzzleEditorStar star in _stars)
			{
				if (star.OverlapsPoint(clickPos))
				{
					return star;
				}
			}

			return null;
		}

		private void LoadInPuzzle(PuzzleData dataToLoad)
		{
			if (dataToLoad != null)
			{
				ResetValues();

				_folderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(AssetDatabase.GetAssetPath(dataToLoad)));
				_puzzleFileName = dataToLoad.name;
				_puzzleId = dataToLoad.PuzzleUniqueId;
				_puzzleName = dataToLoad.PuzzleName;
				_numPuzzleSpinners = dataToLoad.NumSpinners;
				foreach (PuzzleData.StarData starData in dataToLoad.StarDatas)
				{
					PuzzleEditorStar star = new PuzzleEditorStar(starData.FinalColor, _starArea);
					star.SetPositionUsingGamePosition(starData.Position);
					_stars.Add(star);
				}
			}
		}

		private void ResetValues()
		{
			_folderForPuzzleFile = _defaultFolderForPuzzleFile;
			_puzzleFileName = "NewPuzzle";
			_puzzleId = "";
			_puzzleName = "";
			_numPuzzleSpinners = 4;
			_saveModeActive = false;
			_starAreaReferenceImage.Texture = null;
			_starAreaReferenceImage.Color = Color.black;
			_selectedStar = null;
			_stars.Clear();
		}

		private bool PuzzleDataValidForSaving()
		{
			bool validNumStars = _stars.Count >= MIN_STARS_IN_PUZZLE;
			bool validPuzzleId = !string.IsNullOrEmpty(_puzzleId);
			bool validPuzzleName = !string.IsNullOrEmpty(_puzzleName);

			return validNumStars && validPuzzleId && validPuzzleName;
		}
	}
}

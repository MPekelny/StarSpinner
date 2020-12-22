using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EditorWindowStuff
{
	// A helper class to contain the puzzle data part of the puzzle editor window and methods relating to manipulate it, so there can be less code directly in the PuzzleEditorWindow.
	public class PuzzleEditorWindowData
	{
		public const int MIN_STARS_IN_PUZZLE = 15;
		private const string DEFAULT_FOLDER_PATH = "Assets/Content/Puzzles";
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

		private Object _defaultFolderForPuzzleFile = null;

		public Rect StarArea { get; private set; }
		public Object FolderForPuzzleFile { get; set; } = null;
		public string PuzzleFileName { get; set; } = "";
		public string PuzzleId { get; set; } = "";
		public string PuzzleName { get; set; } = "";
		public int NumPuzzleSpinners { get; set; } = 0;
		public List<PuzzleEditorStar> Stars { get; private set; }
		public EditorWindowImage StarAreaReferenceImage { get; private set; }
		public PuzzleToolActionQueue ActionQueue { get; private set; }
		public StarCollisionGrid StarCollisionGrid { get; private set; }

		private bool ValidNumStars => Stars.Count >= MIN_STARS_IN_PUZZLE;
		private bool ValidPuzzleId => !string.IsNullOrEmpty(PuzzleId);
		private bool ValidPuzzleName => !string.IsNullOrEmpty(PuzzleName);
		public bool PuzzleDataValidForSaving => ValidNumStars && ValidPuzzleId && ValidPuzzleName;
		public bool PuzzleDataValidForTesting => ValidNumStars && ValidPuzzleName;

		public PuzzleEditorWindowData()
		{
			StarArea = new Rect(320, 15, 650, 650);
			Stars = new List<PuzzleEditorStar>();
			StarAreaReferenceImage = new EditorWindowImage(null, StarArea.width, StarArea.height, StarArea.center, Color.black);
			ActionQueue = new PuzzleToolActionQueue();
			StarCollisionGrid = new StarCollisionGrid(StarArea);

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

		/// <summary>
		/// Resets all data to default values.
		/// </summary>
		public void ResetValues()
		{
			ActionQueue.ClearQueue();
			FolderForPuzzleFile = _defaultFolderForPuzzleFile;
			PuzzleFileName = "NewPuzzle";
			PuzzleId = "";
			PuzzleName = "";
			NumPuzzleSpinners = 4;
			StarAreaReferenceImage.Texture = null;
			StarAreaReferenceImage.Color = Color.black;
			StarCollisionGrid.ClearGrid();
			Stars.Clear();
		}

		/// <summary>
		/// Writes the data into a puzzle data file.
		/// </summary>
		/// <param name="filePath">The location of where the file should be.</param>
		/// <param name="createNew">If true, the file is meant to be created new. If false, an existing file is meant to be overwritten.</param>
		public void SavePuzzleDataFile(string filePath, bool createNew)
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
				starsAddedForVersion = ActionQueue.GetActionsAsHistoryData(starsDeletedForVersion);
			}

			string imagePath = StarAreaReferenceImage.Texture != null ? AssetDatabase.GetAssetPath(StarAreaReferenceImage.Texture) : "";
			puzzleData.SetDataFromEditorTool(PuzzleId, PuzzleName, NumPuzzleSpinners, Stars, imagePath);
			puzzleData.AddHistoryData(NumPuzzleSpinners, starsAddedForVersion, starsDeletedForVersion);
			EditorUtility.SetDirty(puzzleData);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			ActionQueue.ClearQueue();
		}

		/// <summary>
		/// Takes the puzzle data and sets the window data using it.
		/// </summary>
		public void LoadInPuzzle(PuzzleData dataToLoad)
		{
			if (dataToLoad != null)
			{
				FolderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(AssetDatabase.GetAssetPath(dataToLoad)));
				PuzzleFileName = dataToLoad.name;
				PuzzleId = dataToLoad.PuzzleUniqueId;
				PuzzleName = dataToLoad.PuzzleName;
				if (!string.IsNullOrEmpty(dataToLoad.PuzzleImageReferencePath))
				{
					if (File.Exists(dataToLoad.PuzzleImageReferencePath))
					{
						StarAreaReferenceImage.Texture = AssetDatabase.LoadAssetAtPath<Texture>(dataToLoad.PuzzleImageReferencePath);
						StarAreaReferenceImage.Color = Color.white;
					}
				}

				NumPuzzleSpinners = dataToLoad.NumSpinners;
				foreach (PuzzleData.StarData starData in dataToLoad.StarDatas)
				{
					PuzzleEditorStar star = new PuzzleEditorStar(starData.FinalColor, StarArea);
					star.SetPositionUsingGamePosition(starData.Position);
					StarCollisionGrid.SetStarToGrid(star);
					Stars.Add(star);
				}
			}
		}

		public void AddAddStarAction()
		{
			PuzzleEditorStar starAdded = Stars[Stars.Count - 1];
			PuzzleAddStarAction action = new PuzzleAddStarAction(Stars, StarCollisionGrid, starAdded);
			ActionQueue.AddAction(action);
		}

		public void AddDeleteStarAction(int starDeletedIndex, PuzzleEditorStar deletedStar)
		{
			PuzzleDeleteStarAction action = new PuzzleDeleteStarAction(Stars, StarCollisionGrid, deletedStar, starDeletedIndex);
			ActionQueue.AddAction(action);
		}

		public void AddMoveStarAction(int starChangedIndex, Vector2 beforePosition, Vector2 afterPosition)
		{
			PuzzleMoveStarAction action = new PuzzleMoveStarAction(Stars, StarCollisionGrid, starChangedIndex, beforePosition, afterPosition);
			ActionQueue.AddAction(action);
		}

		public void AddColorStarAction(int starChangedIndex, Color beforeColor, Color afterColor)
		{
			PuzzleColorStarAction action = new PuzzleColorStarAction(Stars, starChangedIndex, beforeColor, afterColor);
			ActionQueue.AddAction(action);
		}

		/// <summary>
		/// Takes all data and puts it into the editor prefs so it can be used by the testing scene and then loaded back after testing is done.
		/// </summary>
		public void PutDataIntoEditorPrefs()
		{
			JSONNode node = new JSONObject();
			node[PUZZLE_ID_KEY] = PuzzleId;
			node[PUZZLE_NAME_KEY] = PuzzleName;
			node[PUZZLE_NUM_SPINNERS_KEY] = NumPuzzleSpinners;
			node[PUZZLE_IMAGE_REF_KEY] = StarAreaReferenceImage.Texture == null ? "null" : AssetDatabase.GetAssetPath(StarAreaReferenceImage.Texture);
			node[PUZZLE_FOLDER_KEY] = AssetDatabase.GetAssetPath(FolderForPuzzleFile);
			node[PUZZLE_FILE_KEY] = PuzzleFileName;

			foreach (PuzzleEditorStar star in Stars)
			{
				JSONObject starData = new JSONObject();
				starData[PUZZLE_STAR_COLOR_R_KEY] = star.EndColour.r;
				starData[PUZZLE_STAR_COLOR_G_KEY] = star.EndColour.g;
				starData[PUZZLE_STAR_COLOR_B_KEY] = star.EndColour.b;
				starData[PUZZLE_STAR_POSITION_X_KEY] = star.GamePosition.x;
				starData[PUZZLE_STAR_POSITION_Y_KEY] = star.GamePosition.y;

				node[PUZZLE_STARS_KEY][-1] = starData;
			}

			node[PUZZLE_ACTION_QUEUE_KEY] = ActionQueue.GetQueueDataAsNode();

			StringBuilder builder = new StringBuilder();
			node.WriteToStringBuilder(builder, 0, 0, JSONTextMode.Compact);
			EditorPrefs.SetString(DATA_BEING_EDITED_PREFS_KEY, builder.ToString());
		}

		/// <summary>
		/// Takes the data from the editor prefs (if there is any), processes it into window data and then clears out the editor pref data.
		/// </summary>
		public void GetDataFromEditorPrefs()
		{
			if (EditorPrefs.HasKey(DATA_BEING_EDITED_PREFS_KEY))
			{
				string json = EditorPrefs.GetString(DATA_BEING_EDITED_PREFS_KEY);
				JSONNode node = JSONNode.Parse(json);
				PuzzleId = node[PUZZLE_ID_KEY].Value;
				PuzzleName = node[PUZZLE_NAME_KEY].Value;
				NumPuzzleSpinners = node[PUZZLE_NUM_SPINNERS_KEY].AsInt;
				string imagePath = node[PUZZLE_IMAGE_REF_KEY].Value;
				if (!string.IsNullOrEmpty(imagePath) && imagePath != "null" && File.Exists(imagePath))
				{
					StarAreaReferenceImage.Texture = AssetDatabase.LoadAssetAtPath<Texture>(imagePath);
					StarAreaReferenceImage.Color = Color.white;
				}

				string folderPath = node[PUZZLE_FOLDER_KEY].Value;
				if (!string.IsNullOrEmpty(imagePath) && folderPath != "null" && Directory.Exists(folderPath))
				{
					FolderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(folderPath));
				}
				
				PuzzleFileName = node[PUZZLE_FILE_KEY].Value;

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

					PuzzleEditorStar star = new PuzzleEditorStar(starColor, StarArea);
					star.SetPositionUsingGamePosition(pos);
					if (StarCollisionGrid.SetStarToGrid(star))
					{
						Stars.Add(star);
					}
				}

				ActionQueue.SetDataFromNode(node[PUZZLE_ACTION_QUEUE_KEY], Stars, StarCollisionGrid);

				EditorPrefs.DeleteKey(DATA_BEING_EDITED_PREFS_KEY);
			}
		}
	}
}

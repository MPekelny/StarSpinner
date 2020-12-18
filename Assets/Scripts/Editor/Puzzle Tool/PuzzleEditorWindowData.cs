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

		public bool PuzzleDataValidForSaving()
		{
			bool validNumStars = Stars.Count >= MIN_STARS_IN_PUZZLE;
			bool validPuzzleId = !string.IsNullOrEmpty(PuzzleId);
			bool validPuzzleName = !string.IsNullOrEmpty(PuzzleName);

			return validNumStars && validPuzzleId && validPuzzleName;
		}

		public bool PuzzleDataValidForTesting()
		{
			bool validNumStars = Stars.Count >= MIN_STARS_IN_PUZZLE;
			bool validPuzzleName = !string.IsNullOrEmpty(PuzzleName);

			return validNumStars && validPuzzleName;
		}

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

				FolderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(Path.GetDirectoryName(node[PUZZLE_FOLDER_KEY].Value));
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

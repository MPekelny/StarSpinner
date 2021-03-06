﻿using SimpleJSON;
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
		private const string PUZZLE_IMAGE_REF_KEY = "puzzle_image_ref";
		private const string PUZZLE_FOLDER_KEY = "puzzle_folder";
		private const string PUZZLE_FILE_KEY = "puzzle_file";
		private const string PUZZLE_ACTION_QUEUE_KEY = "action_queue";
		private const string PUZZLE_SOLVED_IMAGE_KEY = "puzzle_solved_image";

		private Object _defaultFolderForPuzzleFile = null;

		public Rect StarArea { get; private set; }
		public Object FolderForPuzzleFile { get; set; } = null;
		public string PuzzleFileName { get; set; } = "";
		public string PuzzleId { get; set; } = "";
		public string PuzzleName { get; set; } = "";
		public int NumPuzzleSpinners { get; set; } = 0;
		public Sprite PuzzleSolvedImage { get; set; } = null;
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
			PuzzleSolvedImage = null;
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
		/// <param name="resetHistory">If true, instead of adding any changes to the puzzle's version history, resets the history so the current state of the puzzle is version 0.</param>
		public void SavePuzzleDataFile(string filePath, bool createNew, bool resetHistory = false)
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
			List<PuzzleData.StarData> editorStars = new List<PuzzleData.StarData>();
			foreach (PuzzleEditorStar star in Stars)
            {
				editorStars.Add(new PuzzleData.StarData(star.GamePosition, star.EndColour));
            }

			puzzleData.SetDataFromEditorTool(PuzzleId, PuzzleName, NumPuzzleSpinners, PuzzleSolvedImage, editorStars, imagePath);
			if (resetHistory)
			{
				puzzleData.RestartHistory(NumPuzzleSpinners);
			}
			else
			{
				puzzleData.AddHistoryData(NumPuzzleSpinners, starsAddedForVersion, starsDeletedForVersion);
			}
			
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
				PuzzleSolvedImage = dataToLoad.PuzzleSolvedSprite;
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
			node[PuzzleTestScene.PUZZLE_ID_KEY] = PuzzleId;
			node[PuzzleTestScene.PUZZLE_NAME_KEY] = PuzzleName;
			node[PuzzleTestScene.PUZZLE_NUM_SPINNERS_KEY] = NumPuzzleSpinners;
			node[PUZZLE_IMAGE_REF_KEY] = StarAreaReferenceImage.Texture == null ? "null" : AssetDatabase.GetAssetPath(StarAreaReferenceImage.Texture);
			node[PUZZLE_FOLDER_KEY] = AssetDatabase.GetAssetPath(FolderForPuzzleFile);
			node[PUZZLE_FILE_KEY] = PuzzleFileName;
			node[PUZZLE_SOLVED_IMAGE_KEY] = PuzzleSolvedImage == null ? "null" : AssetDatabase.GetAssetPath(PuzzleSolvedImage);

			foreach (PuzzleEditorStar star in Stars)
			{
				JSONObject starData = new JSONObject();
				starData[PuzzleTestScene.PUZZLE_STAR_COLOR_R_KEY] = star.EndColour.r;
				starData[PuzzleTestScene.PUZZLE_STAR_COLOR_G_KEY] = star.EndColour.g;
				starData[PuzzleTestScene.PUZZLE_STAR_COLOR_B_KEY] = star.EndColour.b;
				starData[PuzzleTestScene.PUZZLE_STAR_POSITION_X_KEY] = star.GamePosition.x;
				starData[PuzzleTestScene.PUZZLE_STAR_POSITION_Y_KEY] = star.GamePosition.y;

				node[PuzzleTestScene.PUZZLE_STARS_KEY][-1] = starData;
			}

			node[PUZZLE_ACTION_QUEUE_KEY] = ActionQueue.GetQueueDataAsNode();

			EditorPrefs.SetString(PuzzleTestScene.DATA_BEING_EDITED_PREFS_KEY, node.ToString());
		}

		/// <summary>
		/// Takes the data from the editor prefs (if there is any), processes it into window data and then clears out the editor pref data.
		/// </summary>
		public void GetDataFromEditorPrefs()
		{
			if (EditorPrefs.HasKey(PuzzleTestScene.DATA_BEING_EDITED_PREFS_KEY))
			{
				string json = EditorPrefs.GetString(PuzzleTestScene.DATA_BEING_EDITED_PREFS_KEY);
				JSONNode node = JSONNode.Parse(json);
				PuzzleId = node[PuzzleTestScene.PUZZLE_ID_KEY].Value;
				PuzzleName = node[PuzzleTestScene.PUZZLE_NAME_KEY].Value;
				NumPuzzleSpinners = node[PuzzleTestScene.PUZZLE_NUM_SPINNERS_KEY].AsInt;
				string imagePath = node[PUZZLE_IMAGE_REF_KEY].Value;
				if (!string.IsNullOrEmpty(imagePath) && imagePath != "null" && File.Exists(imagePath))
				{
					StarAreaReferenceImage.Texture = AssetDatabase.LoadAssetAtPath<Texture>(imagePath);
					StarAreaReferenceImage.Color = Color.white;
				}

				string folderPath = node[PUZZLE_FOLDER_KEY].Value;
				if (!string.IsNullOrEmpty(folderPath) && folderPath != "null" && Directory.Exists(folderPath))
				{
					FolderForPuzzleFile = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
				}

				string solvedPath = node[PUZZLE_SOLVED_IMAGE_KEY].Value;
				if (!string.IsNullOrEmpty(solvedPath) && solvedPath != null && File.Exists(solvedPath))
				{
					PuzzleSolvedImage = AssetDatabase.LoadAssetAtPath<Sprite>(solvedPath);
				}
				
				PuzzleFileName = node[PUZZLE_FILE_KEY].Value;

				JSONArray starArray = node[PuzzleTestScene.PUZZLE_STARS_KEY].AsArray;
				for (int i = 0; i < starArray.Count; i++)
				{
					JSONNode starNode = starArray[i];
					float colR = starNode[PuzzleTestScene.PUZZLE_STAR_COLOR_R_KEY];
					float colG = starNode[PuzzleTestScene.PUZZLE_STAR_COLOR_G_KEY];
					float colB = starNode[PuzzleTestScene.PUZZLE_STAR_COLOR_B_KEY];
					float posX = starNode[PuzzleTestScene.PUZZLE_STAR_POSITION_X_KEY];
					float posY = starNode[PuzzleTestScene.PUZZLE_STAR_POSITION_Y_KEY];

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

				EditorPrefs.DeleteKey(PuzzleTestScene.DATA_BEING_EDITED_PREFS_KEY);
			}
		}
	}
}

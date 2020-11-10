using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.TestTools;
using System.Collections;
using System;

namespace Tests.ManagerTests
{
	public class GameManagerTests : IPrebuildSetup, IPostBuildCleanup
	{
		private const string TEST_SCENES_FOLDER = "Assets/Tests/TestScenes";

		private bool _sceneLoaded = false;

		public void Setup()
		{
#if UNITY_EDITOR
			var scenes = new List<EditorBuildSettingsScene>();
			var guids = AssetDatabase.FindAssets("t:Scene", new[] { TEST_SCENES_FOLDER });
			if (guids != null)
			{
				foreach (string guid in guids)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (!string.IsNullOrEmpty(path) && File.Exists(path))
					{
						var scene = new EditorBuildSettingsScene(path, true);
						scenes.Add(scene);
					}
				}
			}

			Debug.Log("Adding test scenes to build settings:\n" + string.Join("\n", scenes.Select(scene => scene.path)));
			EditorBuildSettings.scenes = EditorBuildSettings.scenes.Union(scenes).ToArray();
#endif
		}

		public void Cleanup()
		{
#if UNITY_EDITOR
			EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(scene => !scene.path.StartsWith(TEST_SCENES_FOLDER)).ToArray();
#endif
		}

		[OneTimeSetUp]
		public void SetupForTests()
		{
			SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => 
			{
				_sceneLoaded = true;
			};

			SceneManager.LoadScene("ManagerTesterScene", LoadSceneMode.Single);
		}

		/// <summary>
		/// Just tests that when a scene runs with a game manager in it, the game manager sets itself to the static instance, and that all managers and data object are set.
		/// </summary>
		[UnityTest]
		public IEnumerator TestGameManagerInstanceValid()
		{
			yield return new WaitWhile(() => _sceneLoaded == false);

			Assert.IsNotNull(GameManager.Instance, "GameManager.Instance is null.");
			Assert.IsNotNull(GameManager.Instance.GameDataReference, "GameManager's GameDataReference is null.");
			Assert.IsNotNull(GameManager.Instance.SaveDataManager, "GameManager's SaveDataManager is null.");
			Assert.IsNotNull(GameManager.Instance.ObjectPoolManager, "GameManager's ObjectPoolManager is null.");
			Assert.IsNotNull(GameManager.Instance.ScreenTransitionManager, "GameManager's ScreenTransitionManager is null.");
		}

		/// <summary>
		/// Tests that if a gamemanager is instantiated when the instance is already set, the new gamemanager is destroyed and the already set one is not changed.
		/// </summary>
		[UnityTest]
		public IEnumerator TestSecondManagerHandled()
		{
			yield return new WaitWhile(() => _sceneLoaded == false);

			GameManager previous = GameManager.Instance;

			GameObject manager = (GameObject)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Managers/GameManager.prefab", typeof(GameObject)));
			GameManager testManager = manager.GetComponent<GameManager>();

			// Normally for unit tests, you use IsNull or IsNotNull. However, with gameobjects/components, when they are destroyed, they == null will be true, but the object itself is not actually null.
			// So for tests checking if an object has/has not been destroyed, need to use IsTrue on the object ==/!= null.
			Assert.IsTrue(testManager != null, "Second manager creation failed.");

			yield return new WaitForEndOfFrame();

			Assert.IsTrue(testManager == null, "After waiting a frame, the second game manager isn't destroyed.");
			Assert.IsTrue(GameManager.Instance != null, "After the second game manager creation, gamemanager instance has been destroyed.");
			Assert.IsTrue(GameManager.Instance == previous, "After the second game manager creation, the game manager instance is no longer that same object it was before.");
		}

		/// <summary>
		/// Tests that setting the puzzle index works, and that setting the index out of the range of the puzzle list throws an error.
		/// </summary>
		[UnityTest]
		public IEnumerator TestSettingPuzzleIndex()
		{
			yield return new WaitWhile(() => _sceneLoaded == false);

			int numPuzzles = GameManager.Instance.GameDataReference.PuzzleDatas.Length;

			Assert.DoesNotThrow(() => GameManager.Instance.SetActivePuzzleByIndex(numPuzzles / 2));
			Assert.IsTrue(GameManager.Instance.GetActivePuzzle() == GameManager.Instance.GameDataReference.PuzzleDatas[numPuzzles / 2]);

			Assert.That(() => GameManager.Instance.SetActivePuzzleByIndex(numPuzzles + 3), Throws.TypeOf<ArgumentException>());
			Assert.IsTrue(GameManager.Instance.GetActivePuzzle() == GameManager.Instance.GameDataReference.PuzzleDatas[numPuzzles / 2]);
		}

		/// <summary>
		/// Tests that the gamemanager methods relating to the next puzzle work correctly.
		/// </summary>
		/// <returns></returns>
		[UnityTest]
		public IEnumerator TestNextPuzzleMethods()
		{
			yield return new WaitWhile(() => _sceneLoaded == false);

			int numPuzzles = GameManager.Instance.GameDataReference.PuzzleDatas.Length;

			GameManager.Instance.SetActivePuzzleByIndex(numPuzzles - 2); // Set the puzzle index to the one before the last puzzle.
			Assert.IsTrue(GameManager.Instance.IsThereANextPuzzle()); // Should be a puzzle after the one currently set.
			GameManager.Instance.SetPuzzleIndexToNext(); // Set to the next (last) puzzle.
			Assert.IsTrue(GameManager.Instance.GetActivePuzzle() == GameManager.Instance.GameDataReference.PuzzleDatas[numPuzzles - 1]); // The current puzzle should be the last one.
			Assert.IsFalse(GameManager.Instance.IsThereANextPuzzle()); // Should not be a next puzzle.
			GameManager.Instance.SetPuzzleIndexToNext(); // Set the puzzle to the next puzzle, but should not do anything.
			Assert.IsTrue(GameManager.Instance.GetActivePuzzle() == GameManager.Instance.GameDataReference.PuzzleDatas[numPuzzles - 1]); // The current puzzle should still be the last one.
		}
	}
}


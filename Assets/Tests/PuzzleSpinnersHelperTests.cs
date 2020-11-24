using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PuzzleTests
{
	public class PuzzleSpinnersHelperTests : IPrebuildSetup, IPostBuildCleanup
	{
		private const string TEST_SCENES_FOLDER = "Assets/Tests/TestScenes";
		private bool _sceneLoaded = false;

		TestPuzzleScreen _screen = null;

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
				_screen = GameObject.FindObjectOfType<TestPuzzleScreen>();
				_sceneLoaded = true;
			};

			SceneManager.LoadScene("TestPuzzleScene", LoadSceneMode.Single);
		}

		/// <summary>
		/// Tests that after the screen is cleaned up, there are no spinners and after being set up there are the correct number of spinners.
		/// </summary>
		[UnityTest]
		public IEnumerator TestSpinnerCleanupAndSetup()
		{
			yield return new WaitWhile(() => _sceneLoaded == false);

			// The level would have already been set up automatically, but to make sure we are properly testing, make sure it is properly cleaned up and set up again.
			_screen.TestCleanup();

			yield return new WaitForEndOfFrame();

			Assert.IsTrue(_screen.PuzzleSpinnerHelper.GetSpinnerTransforms().Count == 0);
			Assert.IsTrue(_screen.PuzzleSpinnerHelper.GetSpinnerRotations().Count == 0);

			_screen.TestSetupPuzzle();

			yield return new WaitForEndOfFrame();

			PuzzleData testPuzzleData = GameManager.Instance.GetActivePuzzle();
			Assert.IsTrue(testPuzzleData.NumSpinners == _screen.PuzzleSpinnerHelper.GetSpinnerTransforms().Count);
			Assert.IsTrue(testPuzzleData.NumSpinners == _screen.PuzzleSpinnerHelper.GetSpinnerRotations().Count);
			Assert.IsNotNull(_screen.PuzzleSpinnerHelper.GetRandomSpinnerTransform(out int index));
		}

		/// <summary>
		/// Tests that after being randomly spun, the spinners are split amongst the circle.
		/// </summary>
		[UnityTest]
		public IEnumerator TestRandomSpinSpinners()
		{
			yield return new WaitWhile(() => _sceneLoaded == false);

			// The particular section a spinner ends up in can not be certain, but after the random spin, there should be 1 spinner in each section which is what will be tested.
			_screen.PuzzleSpinnerHelper.RandomSpinSpinners();

			List<float> rotationsAfterSpin = _screen.PuzzleSpinnerHelper.GetSpinnerRotations();

			int numSpinners = rotationsAfterSpin.Count;
			List<Tuple<float, float>> randomRanges = new List<Tuple<float, float>>(numSpinners);
			for (int i = 0; i < numSpinners; i++)
			{
				float rangeMin = 360f / numSpinners * i;
				float rangeMax = 360f / numSpinners * (i + 1);

				// Add a very slight offset to the min and max, so that each spinner stays in the intended initial section even after any potential overlap resolving.
				randomRanges.Add(new Tuple<float, float>(rangeMin + 0.01f, rangeMax - 0.01f));
			}

			foreach (float rotation in rotationsAfterSpin)
			{
				for (int i = 0; i < randomRanges.Count; i++)
				{
					if (rotation >= randomRanges[i].Item1 && rotation <= randomRanges[i].Item2)
					{
						randomRanges.RemoveAt(i);
						break;
					}
				}
			}

			Assert.IsTrue(randomRanges.Count == 0);
		}

		/// <summary>
		/// Tests that the transition end state calls its callback in a timely manner and that all spinners are at 0 rotation after.
		/// </summary>
		[UnityTest]
		public IEnumerator TestSpinnerTransitionToEndState()
		{
			yield return new WaitWhile(() => _sceneLoaded == false);

			bool transitionDone = false;
			float timeSpent = 0f;

			_screen.PuzzleSpinnerHelper.TransitionSpinnersToEndState(() =>
			{
				transitionDone = true;
			});

			yield return new WaitForEndOfFrame();

			while (!transitionDone)
			{
				timeSpent += Time.deltaTime;
				if (timeSpent > _screen.PuzzleSpinnerHelper.GetSpinnerTransitionTime() + 0.25f)
				{
					Assert.IsTrue(false, "Spinner transition took too long.");
				}

				yield return new WaitForEndOfFrame();
			}

			List<float> rotations = _screen.PuzzleSpinnerHelper.GetSpinnerRotations();
			foreach (float rotation in rotations)
			{
				Assert.IsTrue(HelperMethods.EpsilonCheck(rotation, 0f), "After transition a spinner rotation was not 0.");
			}
		}
	}
}


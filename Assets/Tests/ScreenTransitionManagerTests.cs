using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

namespace Tests.ManagerTests
{
	public class ScreenTransitionManagerTests : IPrebuildSetup, IPostBuildCleanup
	{
		private const string TEST_SCENES_FOLDER = "Assets/Tests/TestScenes";

		private ScreenTransitionManager _transitionManager = null;
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
		public void TestSetup()
		{
			GameObject go = (GameObject)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Managers/ScreenTransitionManager.prefab", typeof(GameObject)));
			_transitionManager = go.GetComponent<ScreenTransitionManager>();
			GameObject.DontDestroyOnLoad(_transitionManager);
		}

		[OneTimeTearDown]
		public void Teardown()
		{
			GameObject.Destroy(_transitionManager.gameObject);
		}

		/// <summary>
		/// Tests that calling the fade out does end up calling its callback.
		/// </summary>
		[UnityTest]
		public IEnumerator TestFadeOutHitsCallback()
		{
			float timeInTransition = 0f;
			bool fadedOut = false;
			_transitionManager.FadeOut(() =>
			{
				fadedOut = true;
			});

			Assert.IsFalse(fadedOut, "There should be a delay from when fade out is called before the callback is hit.");

			yield return new WaitForEndOfFrame();

			while (!fadedOut)
			{
				timeInTransition += Time.deltaTime;
				if (timeInTransition >= _transitionManager.TransitionFadeTime + 0.25f) // A little bit of extra time to give some buffer in case timing does not quite match up.
				{
					Assert.IsTrue(false, "Failed to callback for FadeOut that transition happened in a reasonable amount of time.");
				}

				yield return new WaitForEndOfFrame();
			}
		}

		/// <summary>
		/// Tests that calling the fade in does end up calling its callback.
		/// </summary>
		[UnityTest]
		public IEnumerator TestFadeInHitsCallback()
		{
			float timeInTransition = 0f;
			bool fadedIn = false;
			_transitionManager.FadeIn(() =>
			{
				fadedIn = true;
			});

			Assert.IsFalse(fadedIn, "There should be a delay from when fade in is called before the callback is hit.");

			yield return new WaitForEndOfFrame();

			while (!fadedIn)
			{
				timeInTransition += Time.deltaTime;
				if (timeInTransition >= _transitionManager.TransitionFadeTime + 0.25f) // A little bit of extra time to give some buffer in case timing does not quite match up.
				{
					Assert.IsTrue(false, "Failed to callback for FadeIn that transition happened in a reasonable amount of time.");
				}

				yield return new WaitForEndOfFrame();
			}
		}

		/// <summary>
		/// Tests that TransitionScreen does causea the correct scene to be loaded.
		/// </summary>
		[UnityTest]
		public IEnumerator TestSceneTransitionLoadsScene()
		{
			float timeInTransition = 0f;
			bool sceneLoaded = false;
			SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
			{
				sceneLoaded = true;
			};

			_transitionManager.TransitionScreen("ManagerTesterScene");

			yield return new WaitForEndOfFrame();
			while (!sceneLoaded)
			{
				timeInTransition += Time.deltaTime;
				// This can be a bit tricky since scene loading times can vary. But the test scene should take a very little amount of time since it is largely empty.
				// The scene does not really matter, just testing that the scene does end up getting loaded. Like 10 seconds should be good.
				if (timeInTransition >= 10f)
				{
					Assert.IsTrue(false, "The transition manager did not load the scene.");
				}

				yield return new WaitForEndOfFrame();
			}

			Assert.IsTrue(SceneManager.GetActiveScene().name == "ManagerTesterScene");
		}
	}
}


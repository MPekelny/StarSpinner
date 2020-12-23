using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Tests.ManagerTests
{
	public class SaveDataManagerTests
	{
		SaveDataManager _saveManager = null;

		[OneTimeSetUp]
		public void Setup()
		{
			GameObject go = (GameObject)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Managers/SaveData/SaveDataManager.prefab", typeof(GameObject)));
			_saveManager = go.GetComponent<SaveDataManager>();
		}

		[OneTimeTearDown]
		public void Teardown()
		{
			GameObject.Destroy(_saveManager.gameObject);
		}

		/// <summary>
		/// Tests that when a level is saved as complete, the save managerdoes indeed return that that level is marked as complete.
		/// </summary>
		[Test]
		public void TestLevelCompleteWrite()
		{
			_saveManager.SaveLevelCompleted("Test");
			Assert.IsTrue(_saveManager.IsLevelCompleted("Test"));
		}

		/// <summary>
		/// Tests that when a level is removed from being completed, only that one is removed.
		/// </summary>
		[Test]
		public void TestLevelCompleteClear()
		{
			_saveManager.SaveLevelCompleted("Test");
			_saveManager.SaveLevelCompleted("Test2");
			Assert.IsTrue(_saveManager.IsLevelCompleted("Test"));
			Assert.IsTrue(_saveManager.IsLevelCompleted("Test2"));
			_saveManager.RemoveLevelCompled("Test");
			Assert.IsFalse(_saveManager.IsLevelCompleted("Test"));
			Assert.IsTrue(_saveManager.IsLevelCompleted("Test2"));
		}

		/// <summary>
		/// Tests that when ClearAllSaveData is called, it removes all save data.
		/// </summary>
		[Test]
		public void TestClearSaveDataRemovesAll()
		{
			_saveManager.SaveLevelCompleted("Test");
			_saveManager.SaveLevelCompleted("Test2");
			Assert.IsTrue(_saveManager.IsLevelCompleted("Test"));
			Assert.IsTrue(_saveManager.IsLevelCompleted("Test2"));
			_saveManager.ClearAllSaveData();
			Assert.IsFalse(_saveManager.IsLevelCompleted("Test"));
			Assert.IsFalse(_saveManager.IsLevelCompleted("Test2"));
		}
	}
}


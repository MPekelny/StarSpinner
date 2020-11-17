using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.ManagerTests
{
	public class PopupManagerTests
	{
		private TestPopupManager _popupManager = null;

		[OneTimeSetUp]
		public void TestSetup()
		{
			GameObject go = (GameObject)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Tests/TestPrefabs/TestPopupManager.prefab", typeof(GameObject)));
			_popupManager = go.GetComponent<TestPopupManager>();
		}

		/// <summary>
		/// Tests that calling makepopupdata and addbuttondata correctly set the values into the data.
		/// </summary>
		[Test]
		public void TestPopupDataGetsMade()
		{
			PopupData data = _popupManager.MakePopupData("The Test Popup", "Test text for test popup.").AddButtonData("Test Button 1").AddButtonData("Test Button 2");
			Assert.IsNotNull(data, "Test data is null.");
			Assert.IsTrue(data.TitleText == "The Test Popup", "The title text for the test popup data is incorrect.");
			Assert.IsTrue(data.BodyText == "Test text for test popup.", "The body text for the test popup data is incorrect.");
			Assert.IsTrue(data.PopupType == PopupManager.PopupTypes.BasicPopup, "The type for the test popup data is incorrect.");
			Assert.IsTrue(data.ButtonDatas.Count == 2, "The number of buttons for the test popup data is incorrect.");
			Assert.IsTrue(data.ButtonDatas[0].ButtonText == "Test Button 1", "The text for the first button of the test popup data is incorrect.");
			Assert.IsTrue(data.ButtonDatas[1].ButtonText == "Test Button 2", "The text for the second button of the test popup data is incorrect.");
		}

		/// <summary>
		/// Tests that the lifetime of a popup functions correctly: Initially, after being added to the queue it sits in the queue without a popup being made active, then once an update has had a chance to happen, the data is removed and a popup made active (with the correct things set into it),
		/// and then after being dimissed, the queue is still empty and there is no more active popup.
		/// </summary>
		[UnityTest]
		public IEnumerator TestPopupCreationAndDisplay()
		{
			PopupData data = _popupManager.MakePopupData("Test Title", "Test Body");
			_popupManager.AddPopup(data);

			Assert.IsTrue(_popupManager.PopupQueue.Count == 1, "There is not a popup data in queue after being added.");
			Assert.IsFalse(_popupManager.PopupIsActive, "There is an active popup without there being an update after being added.");

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			Assert.IsTrue(_popupManager.PopupQueue.Count == 0, "There is still a popup in the queue after an update after being added.");
			Assert.IsTrue(_popupManager.PopupIsActive, "There is not an active popup after an update after being added.");

			Assert.IsTrue(_popupManager.ActivePopup is BasicPopup, "The test popup is not a BasicPopup");
			BasicPopup popup = (BasicPopup)_popupManager.ActivePopup;
			Assert.IsTrue(popup.TitleString == "Test Title", "The title text of the popup is not correct.");
			Assert.IsTrue(popup.BodyString == "Test Body", "The body text of the popup is not correct.");
			Assert.IsTrue(popup.NumButtonsActive() == 1, "There is not exactly one active button on the popup.");

			_popupManager.ForceDismissActivePopup();

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			Assert.IsTrue(_popupManager.PopupQueue.Count == 0, "There are datas in the queue after the popup was dismissed.");
			Assert.IsFalse(_popupManager.PopupIsActive, "There is an active popup after the popup was dismissed.");
		}

		/// <summary>
		/// Basically the same as the previous test, just that it functions correctly when there are multiple datas queued.
		/// </summary>
		[UnityTest]
		public IEnumerator TestMultiplePopupQueue()
		{
			PopupData data1 = _popupManager.MakePopupData("First Popup Title", "First Popup Body").AddButtonData("Button").AddButtonData("Button 2");
			PopupData data2 = _popupManager.MakePopupData("Second Popup Title", "Second Popup Body");
			_popupManager.AddPopup(data1);
			_popupManager.AddPopup(data2);

			Assert.IsTrue(_popupManager.PopupQueue.Count == 2, "There is not exactly 2 popup datas in queue after being added.");
			Assert.IsFalse(_popupManager.PopupIsActive, "There is an active popup without there being an update after being added.");

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			Assert.IsTrue(_popupManager.PopupQueue.Count == 1, "There is not a single popup in the queue after an update after being added.");
			Assert.IsTrue(_popupManager.PopupIsActive, "There is not an active popup after an update after being added.");

			Assert.IsTrue(_popupManager.ActivePopup is BasicPopup, "The first test popup is not a BasicPopup");
			BasicPopup popup = (BasicPopup)_popupManager.ActivePopup;
			Assert.IsTrue(popup.TitleString == "First Popup Title", "The title text of the first popup is not correct.");
			Assert.IsTrue(popup.BodyString == "First Popup Body", "The body text of the first popup is not correct.");
			Assert.IsTrue(popup.NumButtonsActive() == 2, "There is not exactly two active buttons on the first popup.");

			_popupManager.ForceDismissActivePopup();

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			Assert.IsTrue(_popupManager.PopupQueue.Count == 0, "There is still a popup in the queue after an update after dismiss.");
			Assert.IsTrue(_popupManager.PopupIsActive, "There is not an active popup after an update after dismiss.");

			Assert.IsTrue(_popupManager.ActivePopup is BasicPopup, "The second test popup is not a BasicPopup");
			BasicPopup secondPopup = (BasicPopup)_popupManager.ActivePopup;
			Assert.IsTrue(secondPopup.TitleString == "Second Popup Title", "The title text of the second popup is not correct.");
			Assert.IsTrue(secondPopup.BodyString == "Second Popup Body", "The body text of the second popup is not correct.");
			Assert.IsTrue(secondPopup.NumButtonsActive() == 1, "There is not exactly one active button on the second popup.");

			_popupManager.ForceDismissActivePopup();

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			Assert.IsTrue(_popupManager.PopupQueue.Count == 0, "There are datas in the queue after the second popup was dismissed.");
			Assert.IsFalse(_popupManager.PopupIsActive, "There is an active popup after the second popup was dismissed.");
		}
	}
}


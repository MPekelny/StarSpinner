using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
	[NonSerialized] public string SCREEN_NAME = "TitleScreen";

	public void OnTappedScreen()
	{
		SceneManager.LoadScene(LevelSelectScreen.SCREEN_NAME);
	}
}

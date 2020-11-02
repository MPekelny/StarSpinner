using System;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
	[NonSerialized] public string SCREEN_NAME = "TitleScreen";

	public void OnTappedScreen()
	{
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(LevelSelectScreen.SCREEN_NAME);
	}
}

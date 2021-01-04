using System;
using UnityEngine;

public class CreditsScreen : MonoBehaviour
{
	[NonSerialized] public const string SCREEN_NAME = "CreditsScreen";

	[SerializeField] private TextAsset _credits = null;
	[SerializeField] private TMPro.TextMeshProUGUI _headerText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _creditsText = null;

	public void Start()
	{
		_headerText.text = GameManager.Instance.StringManager.GetStringForKey("game_credits_header");
		_creditsText.text = _credits.text;
	}

	public void BackButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(SettingsScreen.SCREEN_NAME);
	}
}

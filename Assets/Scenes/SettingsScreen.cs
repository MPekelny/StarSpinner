using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
	[NonSerialized] public const string SCREEN_NAME = "SettingsScreen";

	[SerializeField] private TMPro.TextMeshProUGUI _screenHeaderText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _bgmVolumeHeaderText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _bgmVolumeText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _bgmMuteText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _seVolumeHeaderText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _seVolumeText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _seMuteText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _creditsButtonText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _clearSaveDataButtonText = null;

	[SerializeField] private Slider _bgmVolumeSlider = null;
	[SerializeField] private Toggle _bgmMuteToggle = null;
	[SerializeField] private Slider _seVolumeSlider = null;
	[SerializeField] private Toggle _seMuteToggle = null;

	public void Start()
	{
		StringManager stringMan = GameManager.Instance.StringManager;
		_screenHeaderText.text = stringMan.GetStringForKey("settings_header");
		_bgmVolumeHeaderText.text = stringMan.GetStringForKey("settings_music_header");
		_bgmVolumeText.text = stringMan.GetStringForKey("settings_volume");
		_bgmMuteText.text = stringMan.GetStringForKey("settings_mute");
		_seVolumeHeaderText.text = stringMan.GetStringForKey("settings_sound_effects_header");
		_seVolumeText.text = stringMan.GetStringForKey("settings_volume");
		_seMuteText.text = stringMan.GetStringForKey("settings_mute");
		_creditsButtonText.text = stringMan.GetStringForKey("credits_button");
		_clearSaveDataButtonText.text = stringMan.GetStringForKey("clear_save_button");

		_bgmVolumeSlider.SetValueWithoutNotify(GameManager.Instance.AudioManager.BGMVolume);
		_bgmVolumeSlider.onValueChanged.AddListener(BGMVolumeChanged);
		_bgmMuteToggle.SetIsOnWithoutNotify(GameManager.Instance.AudioManager.BGMMuted);
		_bgmMuteToggle.onValueChanged.AddListener(BGMMuteToggled);
		_seVolumeSlider.SetValueWithoutNotify(GameManager.Instance.AudioManager.SEVolume);
		_seVolumeSlider.onValueChanged.AddListener(SEVolumeChanged);
		_seMuteToggle.SetIsOnWithoutNotify(GameManager.Instance.AudioManager.SEMuted);
		_seMuteToggle.onValueChanged.AddListener(SEMuteToggled);
	}

	public void BackButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(LevelSelectScreen.SCREEN_NAME);
	}

	public void BGMVolumeChanged(float volume)
	{
		GameManager.Instance.AudioManager.BGMVolume = volume;
	}

	public void BGMMuteToggled(bool mute)
	{
		GameManager.Instance.AudioManager.BGMMuted = mute;
	}

	public void SEVolumeChanged(float volume)
	{
		GameManager.Instance.AudioManager.SEVolume = volume;
	}

	public void SEMuteToggled(bool mute)
	{
		GameManager.Instance.AudioManager.SEMuted = mute;
	}

	public void CreditsButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);

		StringManager stringMan = GameManager.Instance.StringManager;
		string titleText = stringMan.GetStringForKey("popup_clear_data_title");
		string bodyText = stringMan.GetStringForKey("popup_clear_data_body");

		PopupData data = GameManager.Instance.PopupManager.MakePopupData(titleText, bodyText);

		string yesText = stringMan.GetStringForKey("popup_clear_data_yes");
		data.AddButtonData(yesText, () =>
		{
			GameManager.Instance.SaveDataManager.ClearAllSaveData();
		});

		string noText = stringMan.GetStringForKey("popup_clear_data_no");
		data.AddButtonData(noText);

		GameManager.Instance.PopupManager.AddPopup(data);
	}

	public void ClearSaveButtonPressed()
	{
		GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);
		GameManager.Instance.ScreenTransitionManager.TransitionScreen(CreditsScreen.SCREEN_NAME);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public const string MENU_BGM = "menu_bgm";
	public const string GAME_BGM = "puzzle_bgm";
	public const string GAME_WIN_BGM = "puzzle_solved_bgm";
	public const string BUTTON_SE = "button_pressed";
	public const string PUZZLE_VICTORY_SE = "puzzle_solved";

	private const string BGM_VOLUME_KEY = "bgm_volume";
	private const string SE_VOLUME_KEY = "se_volume";
	private const string BGM_MUTED_KEY = "bgm_muted";
	private const string SE_MUTED_KEY = "se_muted";

	[SerializeField] private AudioData _audioData = null;
	[SerializeField] private AudioSource _bgmSource = null;
	[SerializeField] private AudioSource _soundEffectSource = null;

	private float _bgmVolume = 1f;
	private float _seVolume = 1f;
	private bool _bgmMuted = false;
	private bool _seMuted = false;

	private Dictionary<string, AudioClip> _bgmToKeyDictionary = new Dictionary<string, AudioClip>();
	private Dictionary<string, AudioClip> _soundEffectToKeyDictionary = new Dictionary<string, AudioClip>();

	private bool TooShortToFade(float duration) => duration < 0.05f;

	public float BGMVolume
	{
		get { return _bgmVolume; }
		set
		{
			_bgmVolume = value;
			_bgmSource.volume = value;
			PlayerPrefs.SetFloat(BGM_VOLUME_KEY, value);
			PlayerPrefs.Save();
		}
	}

	public float SEVolume
	{
		get { return _seVolume; }
		set
		{
			_seVolume = value;
			_soundEffectSource.volume = value;
			PlayerPrefs.SetFloat(SE_VOLUME_KEY, value);
			PlayerPrefs.Save();
		}
	}

	public bool BGMMuted
	{
		get { return _bgmMuted; }
		set
		{
			_bgmMuted = value;
			_bgmSource.mute = value;
			PlayerPrefs.SetInt(BGM_MUTED_KEY, Convert.ToInt32(value));
			PlayerPrefs.Save();
		}
	}

	public bool SEMuted
	{
		get { return _seMuted; }
		set
		{
			_seMuted = value;
			_soundEffectSource.mute = value;
			PlayerPrefs.SetInt(SE_MUTED_KEY, Convert.ToInt32(value));
			PlayerPrefs.Save();
		}
	}

    void Start()
    {
		PutAudioDataIntoDictionaries();
		LoadAudioSettings();
    }

	/// <summary>
	/// Plays a sound effect a single time.
	/// </summary>
	/// <param name="soundEffectKey">The key for the clip in the sound effect list in the data to play.</param>
	public void PlaySoundEffect(string soundEffectKey)
	{
		if (!_soundEffectToKeyDictionary.ContainsKey(soundEffectKey))
		{
			Debug.LogError($"Tried to play a sound effect using the key {soundEffectKey}, but a sound effect with that key is not in the data.");
			return;
		}

		_soundEffectSource.PlayOneShot(_soundEffectToKeyDictionary[soundEffectKey]);
	}

	/// <summary>
	/// Switches the bgm in the game to the specified clip, fading the current clip out and then fading in the new one.
	/// </summary>
	/// <param name="bgmKey">The key for the clip in the bgm list in the data to play.</param>
	/// <param name="totalFadeTime">How long the fade transition should last. Note, this is the total duaration for the fade, so the fade out and the fade in are each half of this duration. If the duration is 0 or very short (< 0.05) it will just switch instantly with no fading.</param>
	public void PlayBGM(string bgmKey, float totalFadeTime = 0f)
	{
		void FadeInNextBGM()
		{
			_bgmSource.clip = _bgmToKeyDictionary[bgmKey];
			_bgmSource.volume = 0f;
			_bgmSource.Play();
			StartCoroutine(FadeBgm(_bgmVolume, totalFadeTime / 2f));
		}

		void FadeOutCurrentBGM(bool onlyFadeOut)
		{
			StartCoroutine(FadeBgm(0f, totalFadeTime / 2f, () =>
			{
				if (onlyFadeOut)
				{
					_bgmSource.Stop();
				}
				else
				{
					FadeInNextBGM();
				}
			}));
		}

		if (!_bgmToKeyDictionary.ContainsKey(bgmKey))
		{
			Debug.LogError($"Tried to play a bgm using the key {bgmKey}, but a bgm with that key is not in the data. Only fading out the current bgm.");
			if (_bgmSource.isPlaying)
			{
				if (TooShortToFade(totalFadeTime))
				{
					_bgmSource.Stop();
				}
				else
				{
					FadeOutCurrentBGM(true);
				}
			}

			return;
		}

		// If the fade time is 0 (or just very, very short), don't do any fading, just switch the audio.
		if (TooShortToFade(totalFadeTime))
		{
			_bgmSource.volume = _bgmVolume;
			_bgmSource.clip = _bgmToKeyDictionary[bgmKey];
			_bgmSource.Play();
		}
		else
		{
			if (_bgmSource.isPlaying)
			{
				// Do not fade out/fade in if the current bgm is the same as the one to play.
				if (_bgmToKeyDictionary[bgmKey].name != _bgmSource.clip.name)
				{
					FadeOutCurrentBGM(false);
				}
			}
			else
			{
				FadeInNextBGM();
			}
		}
	}

	/// <summary>
	/// Stops the current bgm from playing, with a fade out.
	/// </summary>
	/// <param name="fadeTime">How long the fade out of the bgm should be before it actually stops. If 0 or very short (<0.05), it will just stop with no fade.</param>
	public void StopBGM(float fadeTime = 0f)
	{
		if (TooShortToFade(fadeTime))
		{
			_bgmSource.Stop();
		}
		else if (_bgmSource.isPlaying)
		{
			StartCoroutine(FadeBgm(0f, fadeTime, () =>
			{
				_bgmSource.Stop();
			}));
		}
	}

	private IEnumerator FadeBgm(float targetVolume, float fadeDuration, Action onFadeComplete = null)
	{
		float currentTime = 0f;
		float startVolume = _bgmSource.volume;

		while (currentTime < fadeDuration)
		{
			currentTime += Time.deltaTime;
			_bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeDuration);
			yield return null;
		}

		onFadeComplete?.Invoke();
	}

	private void PutAudioDataIntoDictionaries()
	{
		for (int i = 0; i < _audioData.BGMAudio.Length; i++)
		{
			if (!_audioData.BGMAudio[i].IsValid)
			{
				Debug.LogError($"Error parsing BGM audio data item {i}, does not have a key or audio clip set. Skipping that item.");
				continue;
			}

			_bgmToKeyDictionary.Add(_audioData.BGMAudio[i].Key, _audioData.BGMAudio[i].AudioAsset);
		}

		for (int i = 0; i < _audioData.SoundEffects.Length; i++)
		{
			if (!_audioData.SoundEffects[i].IsValid)
			{
				Debug.LogError($"Error parsing Sound Effect audio data item {i}, does not have a key or audio clip set. Skipping that item.");
				continue;
			}

			_soundEffectToKeyDictionary.Add(_audioData.SoundEffects[i].Key, _audioData.SoundEffects[i].AudioAsset);
		}
	}

	private void LoadAudioSettings()
	{
		if (PlayerPrefs.HasKey(BGM_VOLUME_KEY))
		{
			float volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY);
			_bgmVolume = volume;
			_bgmSource.volume = volume;
		}
		else
		{
			BGMVolume = 1f;
		}

		if (PlayerPrefs.HasKey(BGM_MUTED_KEY))
		{
			bool muted = Convert.ToBoolean(PlayerPrefs.GetInt(BGM_MUTED_KEY));
			_bgmMuted = muted;
			_bgmSource.mute = muted;
		}
		else
		{
			BGMMuted = false;
		}

		if (PlayerPrefs.HasKey(SE_VOLUME_KEY))
		{
			float volume = PlayerPrefs.GetFloat(SE_VOLUME_KEY);
			_seVolume = volume;
			_soundEffectSource.volume = volume;
		}
		else
		{
			SEVolume = 1f;
		}

		if (PlayerPrefs.HasKey(SE_MUTED_KEY))
		{
			bool muted = Convert.ToBoolean(PlayerPrefs.GetInt(SE_MUTED_KEY));
			_seMuted = muted;
			_soundEffectSource.mute = muted;
		}
		else
		{
			SEMuted = false;
		}
	}
}

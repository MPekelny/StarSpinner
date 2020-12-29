using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Star Spinner/Create Audio Data")]
public class AudioData : ScriptableObject
{
	[System.Serializable]
	public class AudioItem
	{
		[SerializeField] private string _key = "";
		[SerializeField] private AudioClip _audioAsset = null;

		public string Key => _key;
		public AudioClip AudioAsset => _audioAsset;

		public bool IsValid => !string.IsNullOrEmpty(_key) && _audioAsset != null;
	}

	[SerializeField] private AudioItem[] _bgmAudio = null;
	[SerializeField] private AudioItem[] _soundEffects = null;

	public AudioItem[] BGMAudio => _bgmAudio;
	public AudioItem[] SoundEffects => _soundEffects;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Star Spinner/Create Game Strings")]
public class GameStrings : ScriptableObject
{
	[System.Serializable]
	public class KeyStringPair
	{
		[SerializeField] private string _key = "";
		[SerializeField] private string _value = "";

		public string Key => _key;
		public string Value => _value;

		public KeyStringPair(string key, string value)
		{
			_key = key;
			_value = value;
		}
	}

	[SerializeField] private List<KeyStringPair> _stringValues = new List<KeyStringPair>();

	public List<KeyStringPair> StringValues => _stringValues;

#if UNITY_EDITOR
	public void ResetKeyStringPairs()
	{
		_stringValues.Clear();
	}

	public void AddKeyStringPair(KeyStringPair pair)
	{
		_stringValues.Add(pair);
	}
#endif
}

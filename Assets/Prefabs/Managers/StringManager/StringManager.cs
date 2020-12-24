using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringManager : MonoBehaviour
{
	[SerializeField] private GameStrings _stringData = null;

	private Dictionary<string, string> _stringsForKeysDictionary = new Dictionary<string, string>();

    private void Start()
    {
        for (int i = 0; i < _stringData.StringValues.Count; i++)
		{
			_stringsForKeysDictionary.Add(_stringData.StringValues[i].Key, _stringData.StringValues[i].Value);
		}
    }

	public string GetStringForKey(string key)
	{
		if (_stringsForKeysDictionary.ContainsKey(key))
		{
			return _stringsForKeysDictionary[key];
		}
		else
		{
			Debug.LogWarning($"Failed to find string key {key}, returning a default string.");
			return $"><Missing String Key: {key}><";
		}
	}
}

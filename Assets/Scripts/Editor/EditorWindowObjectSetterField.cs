using System;
using UnityEngine;
using UnityEditor;

namespace EditorWindowStuff
{
	public class EditorWindowObjectSetterField<T> where T : UnityEngine.Object
	{
		private const string SELECT_BUTTON_ON_TEXT = "Cancel";

		private T _currentlySetObject = null;
		private Action<T> _onObjectSetCallback = null;

		private float _fieldWidth;
		private string _selectButtonOffText;
		private string _setButtonText;

		private bool _settingIsActive = false;

		public EditorWindowObjectSetterField(string selectButtonOffText, string setButtonText, float fieldWidth, Action<T> onObjectSetCallback)
		{
			_onObjectSetCallback = onObjectSetCallback;
			_fieldWidth = fieldWidth;
			_selectButtonOffText = selectButtonOffText;
			_setButtonText = setButtonText;
		}

		public void Draw()
		{
			GUILayout.BeginHorizontal();

			string selectButtonText = _settingIsActive ? SELECT_BUTTON_ON_TEXT : _selectButtonOffText;
			float selectButtonWidth = _settingIsActive ? (_fieldWidth / 3f) : _fieldWidth;
			if (GUILayout.Button(selectButtonText, GUILayout.Width(selectButtonWidth)))
			{
				_settingIsActive = !_settingIsActive;
			}

			if (_settingIsActive)
			{
				_currentlySetObject = (T)EditorGUILayout.ObjectField(_currentlySetObject, typeof(T), false, GUILayout.Width(_fieldWidth / 3f));
				if (_currentlySetObject != null)
				{
					if (GUILayout.Button(_setButtonText, GUILayout.Width(_fieldWidth / 3f)))
					{
						_onObjectSetCallback?.Invoke(_currentlySetObject);
						_currentlySetObject = null;
						_settingIsActive = false;
					}
				}
			}

			GUILayout.EndHorizontal();
		}
	}
}


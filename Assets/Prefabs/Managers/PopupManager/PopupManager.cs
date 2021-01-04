using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
	public enum PopupTypes
	{
		BasicPopup
	}

	[SerializeField] BasicPopup _basicPopupPrefab = null;
	[SerializeField] GameObject _popupBacking = null;

	private BasicPopup _basicPopupInst = null;

	private Dictionary<PopupTypes, PopupBase> _typeObjectPopupMap = new Dictionary<PopupTypes, PopupBase>();
	protected Queue<PopupData> _popupQueue = new Queue<PopupData>();
	protected PopupBase _activePopup = null;

	public bool PopupIsActive => _activePopup != null;

	private void Start()
	{
		_basicPopupInst = Instantiate<BasicPopup>(_basicPopupPrefab, transform);
		_basicPopupInst.SetBasePopupButtonAction(OnPopupButtonPressed);
		_basicPopupInst.gameObject.SetActive(false);

		_popupBacking.SetActive(false);

		_typeObjectPopupMap.Add(PopupTypes.BasicPopup, _basicPopupInst);
	}

	private void Update()
	{
		if (_popupQueue.Count > 0 && _activePopup == null)
		{
			PopupData nextData = _popupQueue.Dequeue();
			PopupBase popup = _typeObjectPopupMap[nextData.PopupType];
			popup.Initialize(nextData);
			popup.gameObject.SetActive(true);
			_activePopup = popup;
			_popupBacking.SetActive(true);
		}
	}

	/// <summary>
	/// Method that creates a popup data with the needed basic data. One could simply make a new popup data as well, but this does allow you to chain create the datas (i.e. you can go like MakePopupData(...).AddButtonData(...).AddButtonData(...) if you want.
	/// Note, this does not actually add the popup to the queue, AddPopup needs to be called separately to do that.
	/// </summary>
	public PopupData MakePopupData(string titleText, string bodyText, PopupTypes popupType = PopupTypes.BasicPopup)
	{
		PopupData data = new PopupData(titleText, bodyText, popupType);
		return data;
	}

	/// <summary>
	/// Adds the specified popup data to the queue if it is not null.
	/// </summary>
	public void AddPopup(PopupData data)
	{
		if (data != null)
		{
			_popupQueue.Enqueue(data);
		}
		else
		{
			Debug.LogWarning("Attempted to add a null popup to the queue.");
		}
	}

	/// <summary>
	/// Forces the active popup to be closed without calling any of the callbacks on its buttons, as though a button wihtout a callback was hit.
	/// </summary>
	public void ForceDismissActivePopup()
	{
		if (_activePopup != null)
		{
			OnPopupButtonPressed(_activePopup);
		}
	}

	/// <summary>
	/// The basic callback that all popup buttons should call in addition to whatever other call back it does.
	/// </summary>
	private void OnPopupButtonPressed(PopupBase popupFrom)
	{
		if (_activePopup == popupFrom)
		{
			GameManager.Instance.AudioManager.PlaySoundEffect(AudioManager.BUTTON_SE);
			_activePopup.Cleanup();
			_activePopup.gameObject.SetActive(false);
			_activePopup = null;
			_popupBacking.SetActive(false);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPopupManager : PopupManager
{
	public Queue<PopupData> PopupQueue => _popupQueue;

	public PopupBase ActivePopup => _activePopup;
}

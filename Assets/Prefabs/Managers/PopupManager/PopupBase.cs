using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupBase : MonoBehaviour
{
	protected Action<PopupBase> _basePopupButtonAction = null;
	public void SetBasePopupButtonAction(Action<PopupBase> baseAction)
	{
		_basePopupButtonAction = baseAction;
	}

	public abstract void Initialize(PopupData popupData);
	public abstract void Cleanup();
}

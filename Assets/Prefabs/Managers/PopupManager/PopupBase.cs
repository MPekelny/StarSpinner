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

	private void Awake()
	{
		StartCoroutine(DelayForceRebuild());
	}

	private IEnumerator DelayForceRebuild()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		// If the popup was left alone, its shape would be based on how it is in the prefab, which would probably not fit the specific items in the popup.
		// So, after a frame (so the elements in the popup have had the chance to have their sizes updated), force the layout to rebuild so the popup is correctly sized.
		LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
	}

	public abstract void Initialize(PopupData popupData);
	public abstract void Cleanup();
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class BasicPopup : PopupBase
{
	private const string TEXT_FOR_DEFAULT_BUTTON = "Ok";

	[SerializeField] private TMPro.TextMeshProUGUI _titleText = null;
	[SerializeField] private TMPro.TextMeshProUGUI _bodyText = null;
	[SerializeField] private Button[] _popupButtons = null;

	public string TitleString => _titleText.text;
	public string BodyString => _bodyText.text;
	public int NumButtonsActive()
	{
		int numActive = 0;
		foreach (Button button in _popupButtons)
		{
			if (button.gameObject.activeSelf)
			{
				numActive++;
			}
		}

		return numActive;
	}

	public override void Initialize(PopupData popupData)
	{
		_titleText.text = popupData.TitleText;
		_bodyText.text = popupData.BodyText;

		for (int i = 0; i < _popupButtons.Length; i++)
		{
			_popupButtons[i].gameObject.SetActive(false);
		}

		if (popupData.ButtonDatas.Count == 0)
		{
			SetupDefaultButton();
		}
		else
		{
			SetupButtonsForPopup(popupData);
		}
	}

	public override void Cleanup()
	{
		foreach (Button button in _popupButtons)
		{
			button.onClick.RemoveAllListeners();
		}
	}

	private void SetupDefaultButton()
	{
		SetupButton(_popupButtons[0], TEXT_FOR_DEFAULT_BUTTON, null);
	}

	private void SetupButtonsForPopup(PopupData popupData)
	{
		for (int i = 0; i < popupData.ButtonDatas.Count && i < _popupButtons.Length; i++)
		{
			PopupButtonData buttonData = popupData.ButtonDatas[i];
			SetupButton(_popupButtons[i], buttonData.ButtonText, buttonData.ButtonCallback);
		}
	}

	private void SetupButton(Button buttonToSetup, string buttonText, Action buttonCallback)
	{
		buttonToSetup.gameObject.SetActive(true);
		TMPro.TextMeshProUGUI textOfButton = buttonToSetup.GetComponentInChildren<TMPro.TextMeshProUGUI>();
		textOfButton.text = buttonText;

		if (buttonCallback != null)
		{
			buttonToSetup.onClick.AddListener(() => { buttonCallback.Invoke(); });
		}

		buttonToSetup.onClick.AddListener(() => { _basePopupButtonAction?.Invoke(this); });
	}
}

using System;
using System.Collections.Generic;

public class PopupData
{
	public PopupManager.PopupTypes PopupType { get; private set; }
	public string TitleText { get; private set; }
	public string BodyText { get; private set; }
	public List<PopupButtonData> ButtonDatas { get; } = new List<PopupButtonData>();

	public PopupData(string titleText, string bodyText, PopupManager.PopupTypes popupType)
	{
		PopupType = popupType;
		TitleText = titleText;
		BodyText = bodyText;
	}

	public PopupData AddButtonData(string buttonText, Action buttonCallback = null)
	{
		ButtonDatas.Add(new PopupButtonData(buttonText, buttonCallback));

		return this;
	}
}

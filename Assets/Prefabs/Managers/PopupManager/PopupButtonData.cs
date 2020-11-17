using System;

public class PopupButtonData
{
	public string ButtonText { get; private set; }
	public Action ButtonCallback { get; private set; }

	public PopupButtonData(string buttonText, Action buttonCallback)
	{
		ButtonText = buttonText;
		ButtonCallback = buttonCallback;
	}
}

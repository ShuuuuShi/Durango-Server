using Durango.System;

namespace Durango.UI.Control;

public class PortraitCurrencyWidgetHolder : UIWidget, IUIInitializable
{
	void IUIInitializable.Init()
	{
		base.gameObject.SetActive(!Platform.Instance.UsePCUI);
	}
}

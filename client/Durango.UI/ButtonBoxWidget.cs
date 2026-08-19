using System;
using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ButtonBoxWidget : SelectableWidget
{
	[SerializeField]
	private UISprite _arrowSprite;

	public ValueSetting ValueSetting { get; private set; }

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickWidget));
		_arrowSprite.gameObject.SetActive(value: true);
	}

	private void OnClickWidget(GameObject go)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
	}

	public void SetValueSetting(ValueSetting valueSetting)
	{
		ValueSetting = valueSetting;
	}
}

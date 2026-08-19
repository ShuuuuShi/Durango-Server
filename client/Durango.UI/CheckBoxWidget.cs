using System;
using UnityEngine;

namespace Durango.UI;

public class CheckBoxWidget : MonoBehaviour
{
	public Action<bool> ValueChanged;

	[SerializeField]
	private UISprite _checkSprite;

	private bool _value;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickWidget));
	}

	public void SetValue(bool value, bool dispatchEvent)
	{
		_value = value;
		_checkSprite.gameObject.SetActive(value);
		if (dispatchEvent && ValueChanged != null)
		{
			ValueChanged(_value);
		}
	}

	private void OnClickWidget(GameObject go)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		SetValue(!_value, dispatchEvent: true);
	}
}

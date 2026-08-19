using System;
using UnityEngine;

namespace Durango.UI;

public class TextInputOptionWidget : MonoBehaviour
{
	public Action<TextInputOptionWidget, string> OnSubimt;

	[SerializeField]
	private UIInput _input;

	public SettingItem Parent { get; set; }

	public string Value
	{
		get
		{
			return _input.value;
		}
		set
		{
			_input.value = value;
		}
	}

	private void Start()
	{
		_input.defaultText = string.Empty;
		EventDelegate.Set(_input.onSubmit, OnSubmitText);
		UIEventListener uIEventListener = UIEventListener.Get(_input.gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, new UIEventListener.BoolDelegate(OnSelectTextInput));
	}

	private void OnSubmitText()
	{
		if (OnSubimt != null)
		{
			OnSubimt(this, _input.value);
		}
	}

	private void OnSelectTextInput(GameObject obj, bool select)
	{
		if (!select)
		{
			OnSubmitText();
		}
	}
}

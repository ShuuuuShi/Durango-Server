using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class SwitchWidget : MonoBehaviour
{
	public Action<bool> ValueChanged;

	[SerializeField]
	private BinaryToggleSlider _switch;

	private void Start()
	{
		_switch.ValueChanged = OnValueChanged;
	}

	public void SetValue(bool value, bool dispatchEvent, bool immediately = false)
	{
		_switch.Set((!value) ? 0f : 1f, dispatchEvent, !immediately);
	}

	public void SetEnabled(bool enable)
	{
		_switch.SetDisabled(!enable);
	}

	private void OnValueChanged(bool value)
	{
		if (ValueChanged != null)
		{
			ValueChanged(value);
		}
	}
}

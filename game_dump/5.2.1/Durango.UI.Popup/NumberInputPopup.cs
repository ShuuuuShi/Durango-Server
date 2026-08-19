using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Popup;

public class NumberInputPopup : TooltipBase
{
	[SerializeField]
	private UILabel _valueLabel;

	[SerializeField]
	private UISprite _currencyIcon;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private SelectableButton[] _numbers;

	[SerializeField]
	private SelectableButton _00Button;

	[SerializeField]
	private SelectableButton _bsButton;

	[SerializeField]
	private SelectableButton _clearButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	private long _value = -1L;

	private Action<long> _confirmed;

	private long _maxValue;

	protected override void OnAwake()
	{
		for (int i = 0; i < _numbers.Length; i++)
		{
			SelectableButton obj = _numbers[i];
			obj.Clicked = (Action)Delegate.Combine(obj.Clicked, new Action(OnClickNumberButton));
		}
		SelectableButton selectableButton = _00Button;
		selectableButton.Clicked = (Action)Delegate.Combine(selectableButton.Clicked, (Action)delegate
		{
			SetValue(_value * 100);
		});
		SelectableButton bsButton = _bsButton;
		bsButton.Clicked = (Action)Delegate.Combine(bsButton.Clicked, (Action)delegate
		{
			SetValue(_value / 10);
		});
		SelectableButton clearButton = _clearButton;
		clearButton.Clicked = (Action)Delegate.Combine(clearButton.Clicked, (Action)delegate
		{
			SetValue(0L);
		});
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnTryConfirmOnModal));
	}

	private void OnClickNumberButton()
	{
		int num = Array.IndexOf(_numbers, Selectable.Current as SelectableButton);
		if (num != -1)
		{
			SetValue(_value * 10 + num);
		}
	}

	private void SetValue(long value)
	{
		long value2 = _value;
		_value = Math.Min(value, _maxValue);
		if (value2 != _value)
		{
			_valueLabel.text = _value.ToString("N0", T.Culture);
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		if (_confirmed != null)
		{
			_confirmed(_value);
		}
		Hide();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	[ExposedInEditor(null)]
	public void Show(long initialValue, Currency currency, string title, Action<long> onConfirm, long maxValue = 999999999999L)
	{
		SetValue(initialValue);
		_currencyIcon.spriteName = Inventory.GetIcon(currency);
		_titleLabel.text = title;
		_confirmed = onConfirm;
		_maxValue = maxValue;
		Show();
	}
}

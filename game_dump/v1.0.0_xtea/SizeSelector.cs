using System;
using ItemSystem;
using Shared.Economy;
using UnityEngine;

public class SizeSelector : MonoBehaviour
{
	public Action<int> ValueChanged;

	[SerializeField]
	private Selectable _left;

	[SerializeField]
	private Selectable _right;

	[SerializeField]
	private UISpriteLabel _spriteLabel;

	private bool _isTStone;

	private int _value;

	public int Value => _value;

	public int Min { get; private set; }

	public int Max { get; private set; }

	public int Gap { get; private set; }

	private void Start()
	{
		Selectable left = _left;
		left.Clicked = (Action)Delegate.Combine(left.Clicked, new Action(OnMinus));
		Selectable right = _right;
		right.Clicked = (Action)Delegate.Combine(right.Clicked, new Action(OnPlus));
	}

	public void Set(int value, int gap, int min, int max, bool isTStone = false)
	{
		Min = min;
		Max = max;
		Gap = gap;
		_isTStone = isTStone;
		SetValue(value);
	}

	private void SetValue(int value)
	{
		_value = Mathf.Clamp(value, Min, Max);
		_spriteLabel.text = ((!_isTStone) ? _value.ToString() : Inventory.CurrencyFormat(_value, Currency.TStone));
		_left.Disable = _value <= Min;
		_right.Disable = _value >= Max;
		if (ValueChanged != null)
		{
			ValueChanged(_value);
		}
	}

	private void OnMinus()
	{
		if (!Selectable.Current.Disable)
		{
			SetValue(_value - Gap);
		}
	}

	private void OnPlus()
	{
		if (!Selectable.Current.Disable)
		{
			SetValue(_value + Gap);
		}
	}
}

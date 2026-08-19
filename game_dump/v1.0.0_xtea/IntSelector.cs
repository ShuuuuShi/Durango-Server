using System;
using UnityEngine;

public class IntSelector : MonoBehaviour
{
	public static IntSelector Current;

	public Action ValueChanged;

	[SerializeField]
	private GameObject _up;

	[SerializeField]
	private GameObject _down;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private int _interval;

	[SerializeField]
	private string _format;

	private int _value;

	private int _min;

	private int _max;

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			SetValue(value);
		}
	}

	public int Min
	{
		get
		{
			return _min;
		}
		set
		{
			SetMin(value);
		}
	}

	public int Max
	{
		get
		{
			return _max;
		}
		set
		{
			SetMax(value);
		}
	}

	private void Awake()
	{
		UIEventListener.Get(_up).onClick = OnUp;
		UIEventListener.Get(_down).onClick = OnDown;
	}

	public void Set(int val)
	{
		Set(val, int.MinValue, int.MaxValue);
	}

	public void Set(int val, int min, int max)
	{
		_min = min;
		_max = max;
		_value = val;
		Refresh();
	}

	private void SetValue(int val)
	{
		bool flag = _value != val;
		_value = val;
		Refresh();
		if (flag && ValueChanged != null)
		{
			Current = this;
			ValueChanged();
			Current = null;
		}
	}

	private void SetMin(int min)
	{
		_min = min;
		Refresh();
	}

	private void SetMax(int max)
	{
		_max = max;
		Refresh();
	}

	private void Refresh()
	{
		_value = Mathf.Clamp(_value, _min, _max);
		_up.gameObject.SetActive(_value != _max);
		_down.gameObject.SetActive(_value != _min);
		_label.text = ((!string.IsNullOrEmpty(_format)) ? string.Format(_format, _value) : _value.ToString());
	}

	private void OnUp(GameObject go)
	{
		SetValue(_value + Mathf.Max(_interval, 1));
	}

	private void OnDown(GameObject go)
	{
		SetValue(_value - Mathf.Max(_interval, 1));
	}
}

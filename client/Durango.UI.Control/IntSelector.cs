using System;
using UnityEngine;

namespace Durango.UI.Control;

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

	private float _upInterval;

	private float _nextUp;

	private float _downInterval;

	private float _nextDown;

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
		UIEventListener.Get(_up).onPress = OnUp;
		UIEventListener.Get(_down).onPress = OnDown;
	}

	private void OnEnable()
	{
		ResetUp();
		ResetDown();
	}

	private void Update()
	{
		float time = Time.time;
		if (_nextUp > 0f && _nextUp < time)
		{
			Up();
		}
		if (_nextDown > 0f && _nextDown < time)
		{
			Down();
		}
	}

	public void SetFormat(string format)
	{
		_format = format;
		Refresh();
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
		Selectable component = _up.GetComponent<Selectable>();
		if (_value == _max)
		{
			if ((bool)component)
			{
				component.Disabled = true;
			}
			else
			{
				_up.gameObject.SetActive(value: false);
			}
			ResetUp();
		}
		else if ((bool)component)
		{
			component.Disabled = false;
		}
		else
		{
			_up.gameObject.SetActive(value: true);
		}
		Selectable component2 = _down.GetComponent<Selectable>();
		if (_value == _min)
		{
			if ((bool)component2)
			{
				component2.Disabled = true;
			}
			else
			{
				_down.gameObject.SetActive(value: false);
			}
			ResetDown();
		}
		else if ((bool)component2)
		{
			component2.Disabled = false;
		}
		else
		{
			_down.gameObject.SetActive(value: true);
		}
		_label.text = ((!string.IsNullOrEmpty(_format)) ? string.Format(_format, _value) : _value.ToString());
	}

	private void Up()
	{
		_nextUp = Time.time + _upInterval;
		_upInterval *= 0.8f;
		SetValue(_value + Mathf.Max(_interval, 1));
	}

	private void ResetUp()
	{
		_nextUp = 0f;
		_upInterval = 0.2f;
	}

	private void Down()
	{
		_nextDown = Time.time + _downInterval;
		_downInterval *= 0.8f;
		SetValue(_value - Mathf.Max(_interval, 1));
	}

	private void ResetDown()
	{
		_nextDown = 0f;
		_downInterval = 0.2f;
	}

	private void OnUp(GameObject go, bool press)
	{
		if (press)
		{
			Up();
		}
		else
		{
			ResetUp();
		}
	}

	private void OnDown(GameObject go, bool press)
	{
		if (press)
		{
			Down();
		}
		else
		{
			ResetDown();
		}
	}
}

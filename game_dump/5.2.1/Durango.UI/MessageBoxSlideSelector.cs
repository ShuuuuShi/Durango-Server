using System;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MessageBoxSlideSelector : MonoBehaviour
{
	[SerializeField]
	private UILabel _minLabel;

	[SerializeField]
	private UILabel _maxLabel;

	[SerializeField]
	private UILabel _currentLabel;

	[SerializeField]
	private UISprite _currentLabelBg;

	private UISlider _slider;

	private Func<float, string> _toString;

	private bool _isInit;

	private float _unit;

	public float Min { get; private set; }

	public float Max { get; private set; }

	public float Value { get; private set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_slider = GetComponent<UISlider>();
			EventDelegate.Set(_slider.onChange, OnChangeSliderValue);
		}
	}

	private void OnChangeSliderValue()
	{
		float value = _slider.value;
		Value = Mathf.Lerp(Min, Max, value);
		if (_unit > 0f)
		{
			int num = Mathf.RoundToInt((Value - Min) / _unit);
			Value = Min + _unit * (float)num;
			_slider.Set((!(Max <= Min)) ? ((Value - Min) / (Max - Min)) : 1f, notify: false);
		}
		_currentLabel.text = ((_toString != null) ? _toString(Value) : Value.ToString(T.Culture));
	}

	public void Set(float min, float max, float current, Func<float, string> toString)
	{
		Set(min, max, current, 0f, toString);
	}

	public void Set(float min, float max, float current, float unit, Func<float, string> toString)
	{
		Init();
		if (max < min)
		{
			max = min;
		}
		if (current > max || current < min)
		{
			current = Mathf.Clamp(current, min, max);
		}
		_unit = unit;
		if (unit > 0f)
		{
			int num = (int)((max - min) / unit);
			max = min + unit * (float)num;
		}
		Min = min;
		Max = max;
		_toString = toString;
		_minLabel.text = _toString(min);
		_maxLabel.text = _toString(max);
		_currentLabel.text = _maxLabel.text;
		_currentLabelBg.SetDimensions(_currentLabel.width + 30, _currentLabel.height + 30);
		_slider.value = Mathf.Clamp01((!(max <= min)) ? ((current - min) / (max - min)) : 1f);
		OnChangeSliderValue();
	}
}

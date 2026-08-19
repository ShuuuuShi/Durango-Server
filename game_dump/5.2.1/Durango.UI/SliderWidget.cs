using System;
using UnityEngine;

namespace Durango.UI;

public class SliderWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private UISprite _upper;

	[SerializeField]
	private UIWidget _main;

	[SerializeField]
	private UIWidget _circle;

	[SerializeField]
	private UILabel _text;

	private float _max;

	private float _min;

	private float _threshold;

	private bool _showText;

	private Action<float> _changed;

	private void Start()
	{
		UIEventListener.Get(_circle.gameObject).onDrag = delegate
		{
			OnTouched();
		};
		UIEventListener.Get(_main.gameObject).onClick = delegate
		{
			OnTouched();
		};
	}

	private void OnTouched()
	{
		float num = Mathf.Clamp01(((Vector3)NGUIMath.ScreenToPixels(UICamera.currentTouch.pos, _main.transform)).x / (float)_bg.width);
		float num2 = _min + (_max - _min) * num;
		float threshold = _threshold;
		if (threshold > 0f)
		{
			float f = num2 / threshold;
			f = Mathf.Round(f);
			num2 = threshold * f;
		}
		SetValue(num2, dispatchEvent: true);
	}

	public void Initialize(float max, float min, float threshold, bool showText, Action<float> changed)
	{
		_max = max;
		_min = min;
		_threshold = threshold;
		_showText = showText;
		_changed = changed;
		_text.gameObject.SetActive(showText);
	}

	public void SetValue(float value, bool dispatchEvent = false)
	{
		value = Mathf.Clamp(value, _min, _max);
		if (_showText)
		{
			_text.text = ((!(_threshold >= 1f)) ? value.ToString("0.00") : Mathf.FloorToInt(value).ToString());
		}
		float value2 = (value - _min) / (_max - _min);
		value2 = Mathf.Clamp01(value2);
		_upper.width = (int)((float)_bg.width * value2);
		if (_changed != null && dispatchEvent)
		{
			_changed(value);
		}
	}
}

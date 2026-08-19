using UnityEngine;

namespace Durango.UI;

public class IndicatorWidget : UIWidget
{
	public struct Gauge
	{
		public int Delta;

		public int Current;

		public int Max;

		public Gauge(int delta, int current, int max)
		{
			Delta = delta;
			Current = ((current <= 0) ? max : current);
			Max = max;
		}
	}

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UIWidget _gaugeWidget;

	[SerializeField]
	private UISprite _gaugeUpperSprite;

	[SerializeField]
	private UISprite _gaugeSubSprite;

	[SerializeField]
	private GameObject _gaugeLabelWidget;

	[SerializeField]
	private UISprite _bg;

	[SerializeField]
	private int _minGaugePixel = 2;

	public void Set(string icon, string text, Color iconColor, Gauge? gauge)
	{
		_iconSprite.spriteName = icon;
		_iconSprite.color = iconColor;
		_textLabel.text = text;
		int num = _iconSprite.height;
		UISpriteData atlasSprite = _iconSprite.GetAtlasSprite();
		if (atlasSprite == null)
		{
			_iconSprite.SetDimensions(num, num);
		}
		else
		{
			int num2 = atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight;
			int num3 = atlasSprite.height + atlasSprite.paddingTop + atlasSprite.paddingBottom;
			_iconSprite.SetDimensions(num * num2 / num3, num);
		}
		if (gauge.HasValue)
		{
			ShowGauge(gauge.Value);
			if (_bg != null)
			{
				_bg.width = _iconSprite.width + _textLabel.width + _gaugeWidget.width + 46;
			}
		}
		else
		{
			HideGauge();
			if (_bg != null)
			{
				_bg.width = _iconSprite.width + _textLabel.width + 35;
			}
		}
	}

	private void ShowGauge(Gauge gauge)
	{
		float num = (float)gauge.Current / (float)gauge.Max;
		float num2 = (float)(gauge.Current - gauge.Delta) / (float)gauge.Max;
		float num3 = (float)_minGaugePixel / (float)_gaugeSubSprite.width;
		if (num - num2 < num3)
		{
			num2 = num - num3;
		}
		_gaugeUpperSprite.fillAmount = Mathf.Clamp01(num2);
		_gaugeSubSprite.fillAmount = Mathf.Clamp01(num);
		_gaugeLabelWidget.SetActive(gauge.Current == gauge.Max);
		_gaugeWidget.UpdateAnchors();
		_gaugeWidget.gameObject.SetActive(value: true);
	}

	private void HideGauge()
	{
		_gaugeWidget.gameObject.SetActive(value: false);
	}
}

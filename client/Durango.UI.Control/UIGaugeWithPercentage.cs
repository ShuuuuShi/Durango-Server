using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

[RequireComponent(typeof(UIWidget))]
public class UIGaugeWithPercentage : MonoBehaviour
{
	public UIWidget Widget;

	[SerializeField]
	private UILabel _titleText;

	[SerializeField]
	private UILabel _progressPctText;

	[SerializeField]
	private UISprite _progressGaugeFrame;

	[SerializeField]
	private UISprite _progressGaugeContent;

	[SerializeField]
	public Color RatioColor;

	[SerializeField]
	public int padding;

	private void Reset()
	{
		Widget = GetComponent<UIWidget>();
	}

	public UIGaugeWithPercentage SetTitle(string title)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		_titleText.text = title;
		return this;
	}

	public UIGaugeWithPercentage SetGaugeAsPct(double ratio)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		string arg = (ratio * 100.0).ToString("f0").ToEncodedColor(PresetColor.UIYellow);
		_progressPctText.text = $"{arg}%";
		_progressGaugeContent.width = (int)((double)(_progressGaugeFrame.width - padding * 2) * ratio);
		return this;
	}

	public UIGaugeWithPercentage SetGauge(KeyValuePair<double, double> frationRatio)
	{
		return SetGauge(frationRatio.Key, frationRatio.Value);
	}

	public UIGaugeWithPercentage SetGauge(double numerator, double denominator)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		_progressPctText.text = $"{numerator.ToString().ToEncodedColor(PresetColor.UIYellow)}/{denominator}";
		_progressGaugeContent.width = (int)((double)(_progressGaugeFrame.width - padding * 2) * numerator / denominator);
		return this;
	}
}

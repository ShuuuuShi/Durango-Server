using Durango.Network;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class PetGaugeViewerWidget : UIWidget
{
	[SerializeField]
	private UISprite _gaugeSprite;

	[CanBeNull]
	[SerializeField]
	private UILabel _ratioLabel;

	private float? _ratio;

	private Gauge _gauge;

	private Pair<double, double>? _timer;

	private double? _timerFreezeAt;

	private float? Ratio
	{
		set
		{
			if (_ratio != value)
			{
				_ratio = value;
				RefreshGauge(_ratio);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			Ratio = null;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			UpdateGaugeValue();
		}
	}

	private void UpdateGaugeValue()
	{
		if (_gauge != null)
		{
			Ratio = Mathf.Clamp01(_gauge.Ratio());
		}
		else if (_timer.HasValue)
		{
			double valueOrDefault = _timerFreezeAt.GetValueOrDefault(Connections.Frontend.GetPredictedServerTime());
			double num = _timer.Value.Item2 - _timer.Value.Item1;
			double num2 = valueOrDefault - _timer.Value.Item1;
			Ratio = 1f - Mathf.Clamp01((float)(num2 / num));
		}
	}

	private void ClearArguments()
	{
		_gauge = null;
		_timer = null;
	}

	public void Set(float ratio)
	{
		ClearArguments();
		Ratio = ratio;
	}

	public void Set(Gauge gauge)
	{
		ClearArguments();
		_gauge = gauge;
		UpdateGaugeValue();
	}

	public void Set(double since, double until, double? freezeAt)
	{
		ClearArguments();
		_timer = new Pair<double, double>(since, until);
		_timerFreezeAt = freezeAt;
		UpdateGaugeValue();
	}

	private void RefreshGauge(float? ratio)
	{
		if (ratio.HasValue)
		{
			float value = ratio.Value;
			_gaugeSprite.fillAmount = value;
			if (_ratioLabel != null)
			{
				_ratioLabel.text = value.ToString("P0");
			}
		}
	}
}

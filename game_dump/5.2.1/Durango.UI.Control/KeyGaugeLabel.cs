using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public sealed class KeyGaugeLabel : KeyLabelBase
{
	public struct Gauge : IContent
	{
		private double _numerator;

		private double _denominator;

		public Gauge(double numerator, double denominator)
		{
			_numerator = numerator;
			_denominator = denominator;
		}

		public KeyValuePair<double, double> GetValue()
		{
			return new KeyValuePair<double, double>(_numerator, _denominator);
		}
	}

	[SerializeField]
	private UIGaugeWithPercentage _gaugeDisplay;

	private void Reset()
	{
		_gaugeDisplay = GetComponent<UIGaugeWithPercentage>();
	}

	public override KeyLabelBase SetValue(IContent data)
	{
		Gauge gauge = (Gauge)(object)data;
		Init();
		if (_valueLabel == null)
		{
			return this;
		}
		if (_valueLabel.overflowMethod == UILabel.Overflow.ResizeFreely)
		{
			_valueLabel.overflowWidth = 0;
		}
		_gaugeDisplay.SetGauge(gauge.GetValue());
		return this;
	}
}

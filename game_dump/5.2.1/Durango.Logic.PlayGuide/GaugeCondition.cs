using System;

namespace Durango.Logic.PlayGuide;

internal class GaugeCondition : FlowCondition
{
	private readonly float _baseRatio;

	private readonly int _baseValue;

	private readonly Func<float, float, bool> _predicate;

	public GaugeCondition(float ratio, int value, string type)
	{
		_baseRatio = ratio;
		_baseValue = value;
		if (string.Compare(type, "high", StringComparison.OrdinalIgnoreCase) == 0)
		{
			_predicate = HighDiff;
		}
		else
		{
			_predicate = LowDiff;
		}
	}

	private static bool LowDiff(float baseValue, float value)
	{
		return baseValue >= value;
	}

	private static bool HighDiff(float baseValue, float value)
	{
		return baseValue <= value;
	}

	private void LocalPlayer_SurvivalGaugeUpdated(CharacterBehavior player)
	{
		Gauge gauge = player.GetGauge(base.Param);
		if (gauge != null)
		{
			if (_baseRatio >= 0f && _predicate(_baseRatio, gauge.Get() / gauge.Max()))
			{
				Interrupt();
			}
			else if (_baseValue >= 0 && _predicate(_baseValue, gauge.Get()))
			{
				Interrupt();
			}
		}
	}

	protected override void OnRegister()
	{
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += LocalPlayer_SurvivalGaugeUpdated;
	}

	protected override void OnUnregister()
	{
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated -= LocalPlayer_SurvivalGaugeUpdated;
	}
}

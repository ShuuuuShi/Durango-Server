using System;

namespace PlayGuide;

public class GaugeToDo : ToDoBase
{
	private readonly string _targetGauge;

	private readonly float _baseRatio;

	private readonly Func<float, float, bool> _predicate;

	public GaugeToDo(string gaugeName, float ratio, string type)
	{
		_targetGauge = gaugeName;
		_baseRatio = ratio;
		if (string.Compare(type, "high", StringComparison.OrdinalIgnoreCase) == 0)
		{
			_predicate = HighDiff;
		}
		else
		{
			_predicate = LowDiff;
		}
	}

	private static bool HighDiff(float baseValue, float value)
	{
		return baseValue <= value;
	}

	private static bool LowDiff(float baseValue, float value)
	{
		return baseValue >= value;
	}

	private void LocalPlayer_SurvivalGaugeUpdated(CharacterBehavior player)
	{
		Gauge gauge = player.GetGauge(_targetGauge);
		if (gauge != null && _baseRatio >= 0f && _predicate(_baseRatio, gauge.Get() / gauge.Max()))
		{
			CallComplete();
		}
	}

	public override void OnAddItem()
	{
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += LocalPlayer_SurvivalGaugeUpdated;
	}

	public override void OnRemoveItem()
	{
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated -= LocalPlayer_SurvivalGaugeUpdated;
	}
}

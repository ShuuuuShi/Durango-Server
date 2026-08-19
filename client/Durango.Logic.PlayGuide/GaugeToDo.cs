using System;

namespace Durango.Logic.PlayGuide;

public class GaugeToDo : ToDoBase
{
	private readonly string _targetGauge;

	private readonly float _baseRatio;

	private readonly Func<float, float, bool> _predicate;

	public GaugeToDo(string gaugeName, float ratio, bool high)
	{
		_targetGauge = gaugeName;
		_baseRatio = ratio;
		_predicate = ((!high) ? new Func<float, float, bool>(LowDiff) : new Func<float, float, bool>(HighDiff));
	}

	public override void Process()
	{
		if (CheckGaugeCondition(PlayerBehavior.LocalPlayer))
		{
			CallComplete();
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

	private bool CheckGaugeCondition(CharacterBehavior player)
	{
		Gauge gauge = player.GetGauge(_targetGauge);
		return gauge != null && _baseRatio >= 0f && _predicate(_baseRatio, gauge.Get() / gauge.Max());
	}
}

using System;
using Durango.Network;
using Shared.Battle;
using Yaml;

namespace Durango.Logic.Combat;

public class BattleAction
{
	public readonly PlayerAction Data;

	public double? ActivatedUntil;

	public double CooldownSince;

	public double CooldownUntil;

	public double ProhibitedUntil;

	private float? _cooldown;

	private float? _stamina;

	private string _motion;

	public float Cooldown
	{
		get
		{
			return _cooldown.GetValueOrDefault(Data.Meta.Cooldown);
		}
		set
		{
			_cooldown = value;
		}
	}

	public float Stamina
	{
		get
		{
			return _stamina.GetValueOrDefault(Data.Meta.Stamina);
		}
		set
		{
			_stamina = value;
		}
	}

	public string Motion
	{
		get
		{
			return (_motion != null) ? _motion : Data.Meta.Motion;
		}
		set
		{
			_motion = value;
		}
	}

	public float PlaybackRate => Data.Meta.PlaybackRate.GetValueOrDefault(1f);

	public BattleAction(PlayerAction data)
	{
		Data = data;
		ActionActiveCondition activeCondition = data.Meta.ActiveCondition;
		if (activeCondition != null && activeCondition.ActiveType == ActiveType.UseAction)
		{
			ActivatedUntil = 0.0;
		}
	}

	public double? GetDeactivateUntil()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double? result = null;
		if (predictedServerTime < CooldownUntil)
		{
			result = CooldownUntil;
		}
		if (predictedServerTime < ProhibitedUntil)
		{
			result = (result.HasValue ? Math.Max(result.Value, ProhibitedUntil) : ProhibitedUntil);
		}
		return result;
	}
}

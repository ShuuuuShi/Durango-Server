using L10N;
using UnityEngine;

namespace Durango.Logic;

public class Fatigue
{
	public enum State
	{
		None = -1,
		Normal,
		[T.EnumName("지침")]
		Warning,
		[T.EnumName("탈진")]
		Danger
	}

	private Gauge _gauge;

	public float Warning { get; private set; }

	public float Danger { get; private set; }

	public float Velocity => (_gauge != null) ? _gauge.Velocity() : 0f;

	public float Max => (_gauge != null) ? _gauge.RealMax() : 0f;

	public void SetGauge(Gauge gauge, int warning, int danger)
	{
		_gauge = gauge;
		Danger = ((danger != -1) ? ((float)danger) : Max);
		Warning = ((warning != -1) ? ((float)warning) : Danger);
	}

	public float GetRatio(float val)
	{
		return (!(Max > 0f)) ? 1f : (val / Max);
	}

	public float Get()
	{
		return (_gauge != null) ? Mathf.Clamp(_gauge.Get(), 0f, Max) : 0f;
	}

	public float Remain(float val)
	{
		return (val - Get()) / Velocity;
	}

	public State GetState()
	{
		if (_gauge == null)
		{
			return State.Normal;
		}
		float num = _gauge.Get();
		if (num >= Danger)
		{
			return State.Danger;
		}
		if (num > Warning)
		{
			return (GameSystem<StatusEffectSystem>.Instance().GetStatusEffect("fatigue_danger") == null) ? State.Warning : State.Danger;
		}
		return State.Normal;
	}
}

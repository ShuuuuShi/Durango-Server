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

	public float Velocity
	{
		get
		{
			if (_gauge == null)
			{
				return 0f;
			}
			return _gauge.Velocity();
		}
	}

	public float Max
	{
		get
		{
			if (_gauge == null)
			{
				return 0f;
			}
			return _gauge.RealMax();
		}
	}

	public void SetGauge(Gauge gauge, int warning, int danger)
	{
		_gauge = gauge;
		Danger = ((danger != -1) ? ((float)danger) : Max);
		Warning = ((warning != -1) ? ((float)warning) : Danger);
	}

	public float GetRatio(float val)
	{
		if (Max > 0f)
		{
			return val / Max;
		}
		return 1f;
	}

	public float Get()
	{
		if (_gauge == null)
		{
			return 0f;
		}
		return Mathf.Clamp(_gauge.Get(), 0f, Max);
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
			if (GameSystem<StatusEffectSystem>.Instance().GetStatusEffect("fatigue_danger") != null)
			{
				return State.Danger;
			}
			return State.Warning;
		}
		return State.Normal;
	}
}

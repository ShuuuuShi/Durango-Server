using L10N;
using UnityEngine;

namespace EnvironmentData;

public class Fatigue
{
	public enum State
	{
		None = -1,
		Normal,
		[T.EnumName("경고")]
		Warning,
		[T.EnumName("위험")]
		Danger
	}

	public const float ValidMinVelocity = 0.01f;

	public float Warning;

	public float Max;

	public Gauge Gauge;

	public float Velocity => Gauge.Velocity();

	public float GetRatio(float val)
	{
		return (!(Max > 0f)) ? 1f : (val / Max);
	}

	public float Get()
	{
		return Mathf.Clamp(Gauge.Get(), 0f, Max);
	}

	public float Remain(float val)
	{
		return (val - Get()) / Velocity;
	}

	public State GetState()
	{
		float num = Gauge.Get();
		if (num >= Max)
		{
			return State.Danger;
		}
		if (num > Warning)
		{
			return State.Warning;
		}
		return State.Normal;
	}
}

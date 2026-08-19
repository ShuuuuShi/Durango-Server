using UnityEngine;

public struct AnimationClipInfo
{
	public string Name;

	public AnimationState State;

	public float Time
	{
		get
		{
			if (State == null)
			{
				return 0f;
			}
			return Mathf.Repeat(State.time, State.length);
		}
	}

	public float Length
	{
		get
		{
			if (State == null)
			{
				return 0f;
			}
			return State.length;
		}
	}

	public bool IsLoop
	{
		get
		{
			if (State != null)
			{
				return State.wrapMode == WrapMode.Loop;
			}
			return false;
		}
	}

	public float PlaybackRate
	{
		get
		{
			if (State == null)
			{
				return 1f;
			}
			return State.speed;
		}
	}
}

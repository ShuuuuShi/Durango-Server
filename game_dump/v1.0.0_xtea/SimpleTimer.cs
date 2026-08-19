using UnityEngine;

public class SimpleTimer
{
	private float _nextTime;

	private readonly float _period;

	public SimpleTimer(float period)
	{
		_period = period;
		_nextTime = Time.time + _period;
	}

	public bool CheckTime()
	{
		if (Time.time >= _nextTime)
		{
			_nextTime = Time.time + _period;
			return true;
		}
		return false;
	}
}

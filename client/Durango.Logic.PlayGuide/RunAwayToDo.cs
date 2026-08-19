using UnityEngine;

namespace Durango.Logic.PlayGuide;

public class RunAwayToDo : ToDoBase
{
	private float _timeSum;

	private float _lastCheckTime;

	private readonly float _checkTime;

	public RunAwayToDo(float checkTime)
	{
		_checkTime = checkTime;
	}

	public override void Process()
	{
		float time = Time.time;
		float num = time - _lastCheckTime;
		if (!(num < 1f))
		{
			_lastCheckTime = time;
			_timeSum += num;
			if (Util.CheckNearAnimal(0))
			{
				_timeSum = 0f;
			}
			else if (_timeSum > _checkTime)
			{
				CallComplete();
			}
		}
	}

	public override void OnAddItem()
	{
		_lastCheckTime = Time.time;
	}
}

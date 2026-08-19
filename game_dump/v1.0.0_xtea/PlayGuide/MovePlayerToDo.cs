using UnityEngine;

namespace PlayGuide;

public class MovePlayerToDo : ToDoBase
{
	private float _completeTime = -1f;

	public float CheckTime { get; set; }

	private void PlayerController_MoveStarted()
	{
		KSingleton<PlayerController>.Instance().MoveStarted -= PlayerController_MoveStarted;
		_completeTime = Time.time + CheckTime;
	}

	public override void Process()
	{
		if (_completeTime > 0f && Time.time >= _completeTime)
		{
			CallComplete();
		}
	}

	public override void OnAddItem()
	{
		KSingleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
	}

	public override void OnRemoveItem()
	{
		KSingleton<PlayerController>.Instance().MoveStarted -= PlayerController_MoveStarted;
		_completeTime = -1f;
	}
}

using Durango.Utils;
using UnityEngine;

namespace Durango.Logic.PlayGuide;

public class MovePlayerToDo : ToDoBase
{
	private float _completeTime = -1f;

	public float CheckTime { get; set; }

	private void PlayerController_MoveStarted()
	{
		Singleton<PlayerController>.Instance().MoveStarted -= PlayerController_MoveStarted;
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
		Singleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
	}

	public override void OnRemoveItem()
	{
		Singleton<PlayerController>.Instance().MoveStarted -= PlayerController_MoveStarted;
		_completeTime = -1f;
	}
}

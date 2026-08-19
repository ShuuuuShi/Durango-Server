using UnityEngine;

namespace PlayGuide;

internal class ReachPosToDo : ToDoBase
{
	private readonly Vector2 _pos;

	private readonly float _triggerRadius;

	public ReachPosToDo(Vector2 pos, float triggerRadius)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_pos = pos;
		_triggerRadius = triggerRadius;
	}

	public override void OnAddItem()
	{
		KSingleton<PlayerController>.Instance().MoveEnded += PlayerController_MoveEnded;
	}

	public override void OnRemoveItem()
	{
		KSingleton<PlayerController>.Instance().MoveEnded -= PlayerController_MoveEnded;
	}

	private void PlayerController_MoveEnded()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = TerrainA6.TilePositionToClientPosition(_pos);
		Vector3 val2 = val - PlayerBehavior.LocalPlayer.CurrentPosition;
		if (((Vector3)(ref val2)).magnitude <= _triggerRadius)
		{
			CallComplete();
		}
	}
}

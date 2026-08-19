using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.Logic.PlayGuide;

public class FindTileToDo : ToDoBase
{
	private readonly Vector2 _pos;

	private readonly float _triggerRadius;

	public FindTileToDo(Vector2 pos, float triggerRadius)
	{
		_pos = pos;
		_triggerRadius = triggerRadius;
	}

	public override void OnAddItem()
	{
		Singleton<PlayerController>.Instance().MoveEnded += PlayerController_MoveEnded;
	}

	public override void OnRemoveItem()
	{
		Singleton<PlayerController>.Instance().MoveEnded -= PlayerController_MoveEnded;
	}

	private void PlayerController_MoveEnded()
	{
		Vector3 vector = Durango.Terrain.Util.TilePositionToClientPosition(_pos);
		if ((vector - PlayerBehavior.LocalPlayer.CurrentPosition).magnitude <= _triggerRadius)
		{
			CallComplete();
		}
	}
}

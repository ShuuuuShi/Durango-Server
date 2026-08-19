using UnityEngine;

namespace PlayGuide;

internal class FindImmovable : ToDoBase
{
	private readonly int[] _types;

	private readonly float _radius;

	public FindImmovable(string id, float radius)
	{
		_types = TerrainDataHelper.ParseEntityTypes(id);
		_radius = radius;
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
		if ((Object)(object)Util.GetNearImmovable(_types, _radius) != (Object)null)
		{
			CallComplete();
		}
	}
}

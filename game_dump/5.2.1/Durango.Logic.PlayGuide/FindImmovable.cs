using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;

namespace Durango.Logic.PlayGuide;

public class FindImmovable : ToDoBase
{
	private readonly int[] _types;

	private readonly float _radius;

	public FindImmovable(string id, float radius)
	{
		_types = DataHelper.ParseEntityTypes(id);
		_radius = radius;
	}

	public override void OnAddItem()
	{
		Singleton<PlayerController>.Instance().MoveEnded += PlayerController_MoveEnded;
		Singleton<ArtifactManager>.Instance().Added += ArtifactManager_Added;
	}

	public override void OnRemoveItem()
	{
		Singleton<PlayerController>.Instance().MoveEnded -= PlayerController_MoveEnded;
		Singleton<ArtifactManager>.Instance().Added -= ArtifactManager_Added;
	}

	private void PlayerController_MoveEnded()
	{
		if (Util.GetNearestImmovable(_types, _radius) != null)
		{
			CallComplete();
		}
	}

	private void ArtifactManager_Added(Artifact artifact)
	{
		if (_types.Contains(artifact.EntityType) && (Durango.Terrain.Util.TilePositionToClientPosition(artifact.WorldTile) - PlayerBehavior.LocalPlayer.CurrentPosition).sqrMagnitude < _radius * _radius)
		{
			CallComplete();
		}
	}
}

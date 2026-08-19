using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;

namespace Durango.Logic.PlayGuide;

internal class FindImmovableCondition : FlowCondition
{
	private readonly int[] _types;

	private readonly float _radius;

	public FindImmovableCondition(string param)
	{
		if (!string.IsNullOrEmpty(param))
		{
			string[] array = param.Split(':');
			_types = DataHelper.ParseEntityTypes(array[0]);
			if (array.Length > 1)
			{
				float.TryParse(array[1], out _radius);
			}
			else
			{
				_radius = 600f;
			}
		}
	}

	protected override void OnRegister()
	{
		Singleton<PlayerController>.Instance().MoveEnded += PlayerController_MoveEnded;
		Singleton<ArtifactManager>.Instance().Added += ArtifactManager_Added;
	}

	protected override void OnUnregister()
	{
		Singleton<PlayerController>.Instance().MoveEnded -= PlayerController_MoveEnded;
		Singleton<ArtifactManager>.Instance().Added -= ArtifactManager_Added;
	}

	private void PlayerController_MoveEnded()
	{
		if (Util.GetNearestImmovable(_types, _radius) != null)
		{
			Interrupt();
		}
	}

	private void ArtifactManager_Added(Artifact artifact)
	{
		if (_types.Contains(artifact.EntityType) && (Durango.Terrain.Util.TilePositionToClientPosition(artifact.WorldTile) - PlayerBehavior.LocalPlayer.CurrentPosition).sqrMagnitude < _radius * _radius)
		{
			Interrupt();
		}
	}
}

using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Region;

namespace Durango.Logic.PlayGuide;

public class FindBiomeToDo : ToDoBase
{
	[NotNull]
	private readonly Biome[] _biomes;

	private readonly int _tileRadius;

	public FindBiomeToDo(string id, float radius)
	{
		_biomes = DataHelper.ParseBiome(id);
		_tileRadius = (int)(radius / 200f);
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
		if (Singleton<TerrainBase>.Instance().HasBiomeInSquareRange(_biomes, PlayerBehavior.LocalPlayer.CurrentPosition, _tileRadius))
		{
			CallComplete();
		}
	}
}

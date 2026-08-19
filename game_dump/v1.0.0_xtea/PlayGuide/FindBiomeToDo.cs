using TerrainData;

namespace PlayGuide;

internal class FindBiomeToDo : ToDoBase
{
	private readonly Biome[] _biomes;

	public FindBiomeToDo(string id)
	{
		_biomes = TerrainDataHelper.ParseBiome(id);
	}

	public override void OnAddItem()
	{
		if (_biomes != null)
		{
			KSingleton<PlayerController>.Instance().MoveEnded += PlayerController_MoveEnded;
		}
	}

	public override void OnRemoveItem()
	{
		KSingleton<PlayerController>.Instance().MoveEnded -= PlayerController_MoveEnded;
	}

	private void PlayerController_MoveEnded()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (TerrainA6.HasBiomeInSquareRange(_biomes, PlayerBehavior.LocalPlayer.CurrentPosition, 3))
		{
			CallComplete();
		}
	}
}

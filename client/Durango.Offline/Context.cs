using JetBrains.Annotations;

namespace Durango.Offline;

public class Context
{
	[NotNull]
	public readonly WorldContext World;

	[NotNull]
	public readonly PlayerContext Player;

	public int PlayerSlot => World.PlayerSlot;

	public string EntityId => Player.EntityId;

	public Context(WorldContext world, PlayerContext player)
	{
		World = world;
		Player = player;
	}
}

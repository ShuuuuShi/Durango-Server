using Shared.Teleport;

namespace PlayGuide;

internal class WarpToDo : ToDoBase
{
	public override void OnAddItem()
	{
		KSingleton<PlayerManager>.Instance().Teleported += PlayerManager_Teleported;
	}

	public override void OnRemoveItem()
	{
		KSingleton<PlayerManager>.Instance().Teleported -= PlayerManager_Teleported;
	}

	private void PlayerManager_Teleported(TeleportType type)
	{
		if (type == TeleportType.Warp || type == TeleportType.WarpBack)
		{
			CallComplete();
		}
	}
}

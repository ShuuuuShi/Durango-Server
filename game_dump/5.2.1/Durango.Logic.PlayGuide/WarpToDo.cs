using Durango.Utils;
using Shared.Teleport;

namespace Durango.Logic.PlayGuide;

public class WarpToDo : ToDoBase
{
	public override void OnAddItem()
	{
		Singleton<PlayerManager>.Instance().Teleported += PlayerManager_Teleported;
	}

	public override void OnRemoveItem()
	{
		Singleton<PlayerManager>.Instance().Teleported -= PlayerManager_Teleported;
	}

	private void PlayerManager_Teleported(TeleportType type)
	{
		if (type == TeleportType.Warp || type == TeleportType.WarpBack)
		{
			CallComplete();
		}
	}
}

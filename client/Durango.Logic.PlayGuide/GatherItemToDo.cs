using Messages;

namespace Durango.Logic.PlayGuide;

public class GatherItemToDo : ToDoBase
{
	private int _gatherCount;

	public GatherItemToDo(int targetCount)
	{
		base.TargetProgress = ((targetCount <= 0) ? 1 : targetCount);
		_gatherCount = 0;
	}

	private void GatheringSystem_ItemCollected(Messages.Item item)
	{
		_gatherCount++;
		CallProgressChange(_gatherCount);
	}

	public override void OnAddItem()
	{
		GameSystem<GatheringSystem>.Instance().ItemCollected += GatheringSystem_ItemCollected;
	}

	public override void OnRemoveItem()
	{
		GameSystem<GatheringSystem>.Instance().ItemCollected -= GatheringSystem_ItemCollected;
	}
}

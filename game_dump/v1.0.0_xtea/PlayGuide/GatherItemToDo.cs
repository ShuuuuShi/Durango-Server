using ItemSystem;

namespace PlayGuide;

public class GatherItemToDo : ToDoBase
{
	private int _gatherCount;

	public GatherItemToDo(int targetCount)
	{
		base.TargetProgress = ((targetCount <= 0) ? 1 : targetCount);
		_gatherCount = 0;
	}

	private void OnCollectItem(ItemData item)
	{
		_gatherCount++;
		CallProgressChange(_gatherCount);
	}

	public override void OnAddItem()
	{
		GameSystem<InventorySystem>.Instance().OnCollectItem += OnCollectItem;
	}

	public override void OnRemoveItem()
	{
		GameSystem<InventorySystem>.Instance().OnCollectItem -= OnCollectItem;
	}
}

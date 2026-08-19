using Durango.Logic.Item;
using L10N;

namespace Durango.Logic.PlayGuide;

public class GetSlotItemToDo : ToDoBase
{
	public readonly OrTagFilter RequiredTags;

	public readonly OrTagFilter RequiredMaterials;

	public GetSlotItemToDo(OrTagFilter tags, OrTagFilter materials, string slotName)
	{
		RequiredTags = tags;
		RequiredMaterials = materials;
		base.LocalText = T._("<em>{0}</em> 구하기", slotName);
	}

	protected void OnUpdateInventory()
	{
		int taggedItemCount = GameSystem<InventorySystem>.Instance().GetTaggedItemCount(RequiredTags, RequiredMaterials);
		CallProgressChange(taggedItemCount);
	}

	public override void OnAddItem()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
		OnUpdateInventory();
	}

	public override void OnRemoveItem()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
	}
}

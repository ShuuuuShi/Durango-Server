using ItemSystem;
using L10N;

namespace PlayGuide;

public class GetSlotItemToDo : ToDoBase
{
	public readonly TagFilter[] RequiredTags;

	public readonly TagFilter[] RequiredMaterials;

	public GetSlotItemToDo(TagFilter[] tags, TagFilter[] materials, string slotName)
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
		GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		OnUpdateInventory();
	}

	public override void OnRemoveItem()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
	}
}

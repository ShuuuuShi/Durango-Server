using Durango.Logic.Item;
using Messages;

namespace Crafting;

public struct TechSupportTarget
{
	public ItemData Item;

	public int ReformSlotIndex;

	public TechSupportTarget(ItemData item, int slotIndex)
	{
		Item = item;
		ReformSlotIndex = slotIndex;
	}

	public ReformSlot? GetReformSlot()
	{
		return GetReformSlot(Item, ReformSlotIndex);
	}

	public static bool HasReformSlot(ItemData item)
	{
		if (item != null)
		{
			return item.ReformSlots.Count > 0;
		}
		return false;
	}

	public static bool HasEmptyReformSlot(ItemData item)
	{
		if (item != null)
		{
			foreach (ReformSlot reformSlot in item.ReformSlots)
			{
				if (string.IsNullOrEmpty(reformSlot.RecipeId))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static ReformSlot? GetReformSlot(ItemData item, int slotIndex)
	{
		if (item != null && 0 <= slotIndex && slotIndex <= item.ReformSlots.Count)
		{
			return item.ReformSlots[slotIndex];
		}
		return null;
	}
}

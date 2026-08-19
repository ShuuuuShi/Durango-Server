namespace ItemSystem;

public interface IItemSlot
{
	bool IsSuitableItem(ItemData itemData, bool ignoreLevel = false);
}

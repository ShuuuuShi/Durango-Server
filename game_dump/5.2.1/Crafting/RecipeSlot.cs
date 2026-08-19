using Durango.Logic.Item;

namespace Crafting;

public class RecipeSlot : ItemSlot
{
	public enum Type
	{
		General,
		ModifyBase,
		ReformBase
	}

	public int CountMax;

	public Type SlotType { get; set; }

	public bool IsBase => base.Id == "base";

	public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)
	{
		bool flag = base.IsSuitableItem(itemData, ignoreSubReason);
		if (!flag)
		{
			return false;
		}
		switch (SlotType)
		{
		case Type.ModifyBase:
			if (!ignoreSubReason)
			{
				flag = itemData.ModifiableCount > 0;
			}
			break;
		case Type.ReformBase:
			flag = TechSupportTarget.HasReformSlot(itemData);
			if (flag && !ignoreSubReason)
			{
				flag = TechSupportTarget.HasEmptyReformSlot(itemData);
			}
			break;
		}
		return flag;
	}
}

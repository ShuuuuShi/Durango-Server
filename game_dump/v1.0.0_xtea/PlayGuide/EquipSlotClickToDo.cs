using UnityEngine;

namespace PlayGuide;

public class EquipSlotClickToDo : ToDoBase
{
	private readonly EquipSystem.Slot _slot;

	public EquipSlotClickToDo(string slot)
	{
		_slot = slot.ToEnum(EquipSystem.Slot.Precious);
	}

	public override void OnAddItem()
	{
		EquipGroup equipGroup = UIManager.FindScript<EquipGroup>();
		if ((Object)(object)equipGroup != (Object)null)
		{
			equipGroup.EquipSlotClicked += EquipGroup_EquipSlotClicked;
		}
		else
		{
			CallComplete();
		}
	}

	private void EquipGroup_EquipSlotClicked(EquipSystem.Slot slot)
	{
		if (slot == _slot)
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		EquipGroup equipGroup = UIManager.FindScript<EquipGroup>();
		if ((Object)(object)equipGroup != (Object)null)
		{
			equipGroup.EquipSlotClicked -= EquipGroup_EquipSlotClicked;
		}
	}
}

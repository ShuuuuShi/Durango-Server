using Durango.Logic.Item;
using Durango.Logic.PlayGuide;
using Durango.UI.Popup;

namespace Durango.UI;

public class ItemSlotTodo : ToDoBase
{
	private ItemSlot _slot;

	public override bool IsVisibleProgress => true;

	public void Set(ItemSlot slot)
	{
		_slot = slot;
		base.LocalText = slot.Name;
		base.TargetProgress = slot.Count;
	}

	public void Set(OrTagFilter tool)
	{
		_slot = null;
		base.LocalText = Durango.Logic.Item.Util.LocalizedTagRequiredMsg(tool, showLevel: false);
		base.TargetProgress = 1;
	}

	public override bool OnClicked()
	{
		if (_slot != null)
		{
			SlotInfoPopup slotInfoPopup = UIManager.Popup.Tooltip<SlotInfoPopup>();
			slotInfoPopup.Set(_slot.Name, _slot.RequiredLevel, _slot.AllowedTags, _slot.AllowedMaterials, _slot.SlotSourceInfos);
			slotInfoPopup.Show();
		}
		return true;
	}
}

using Durango.Logic.Item;
using Durango.UI.Popup;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

public class EquipWidget : EquipWidgetBase
{
	protected override void SelectEquipPreset(EquipSlotType presetType)
	{
		base.SelectEquipPreset(presetType);
		SelectSlot(base.SelectedSlot);
	}

	protected override void ItemList_OnUpdateSelectItem()
	{
		ItemData lastSelectedItem = _itemList.LastSelectedItem;
		if (lastSelectedItem == null)
		{
			if (base.LastSelected != null)
			{
				ToggleEquipLastSelectedItem();
			}
			return;
		}
		base.LastSelected = lastSelectedItem;
		RefreshEquipButton();
		ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
		itemInfoTooltip.Sign = -1;
		itemInfoTooltip.Set(base.LastSelected);
		itemInfoTooltip.Show(_itemList.gameObject, Vector2.zero, 60f);
		itemInfoTooltip.HideArrow();
	}
}

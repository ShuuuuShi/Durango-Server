using System.Collections.Generic;
using ItemSystem;
using MenuData;
using UnityEngine;

namespace PlayGuide;

public class ClickTargetLocatorInventory : ClickTargetLocator
{
	private LeftMenuListGroup _leftMenuGroup;

	private InventoryGroup _inventoryGroup;

	private ItemIcon2 _selectedItem;

	protected override void OnInitialized()
	{
		_inventoryGroup = UIManager.FindScript<InventoryGroup>();
		_leftMenuGroup = UIManager.FindScript<LeftMenuListGroup>();
	}

	protected override string SelectPhase()
	{
		if ((Object)(object)_inventoryGroup != (Object)null && _inventoryGroup.IsOpen)
		{
			if ((Object)(object)_selectedItem != (Object)null && _selectedItem.Selected)
			{
				return "use_button";
			}
			return "item_select";
		}
		if ((Object)(object)_leftMenuGroup != (Object)null && _leftMenuGroup.IsMenuVisible())
		{
			return "inventory_menu";
		}
		return "bottom_left_menu";
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "bottom_left_menu":
			base.TargetTransform = ((!((Object)(object)_leftMenuGroup != (Object)null)) ? null : _leftMenuGroup.GetBottomLeftMenuTransform());
			break;
		case "inventory_menu":
			base.TargetTransform = _leftMenuGroup.GetMenuTransform(MenuType.Inventory);
			CurrentClickTarget.x = 0.02f;
			CurrentClickTarget.y = -0.01f;
			break;
		case "item_select":
		{
			List<ItemData> list = GameSystem<InventorySystem>.Instance().FilteringByTag(CurrentClickTarget.id);
			list.Sort(ItemSystem.Util.GetItemComparison(ItemSystem.Util.SortOption.Default));
			for (int i = 0; i < list.Count; i++)
			{
				ItemData item = list[i];
				ItemIcon2 itemIcon = _inventoryGroup.FindItem(item);
				if ((Object)(object)itemIcon != (Object)null)
				{
					_selectedItem = itemIcon;
					base.TargetTransform = ((Component)itemIcon).transform;
					break;
				}
			}
			break;
		}
		case "use_button":
			base.TargetTransform = _inventoryGroup.GetUseButtonTransform();
			break;
		}
	}
}

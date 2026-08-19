using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Item;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorInventory : LocatorMenu
{
	private InventoryGroup _inventoryGroup;

	private ItemIconWidget _selectedItem;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_inventoryGroup = UIManager.Inventory;
		SetMenuType(MenuType.Inventory);
	}

	protected override string SelectPhase()
	{
		if (_inventoryGroup != null && _inventoryGroup.IsOpened)
		{
			if (_selectedItem != null && _selectedItem.Selected)
			{
				return "use_button";
			}
			return "item_select";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "item_select":
		{
			List<ItemData> list = GameSystem<InventorySystem>.Instance().FilteringByTag(base.CurrentParameter.id);
			list.Sort(Util.GetItemComparison(Util.SortOption.Default));
			for (int i = 0; i < list.Count; i++)
			{
				ItemData itemData = list[i];
				string id = ((itemData != null) ? itemData.Id : string.Empty);
				ItemIconWidget itemIconWidget = _inventoryGroup.FindItem(id);
				if (itemIconWidget != null)
				{
					_selectedItem = itemIconWidget;
					base.TargetTransform = itemIconWidget.transform;
					break;
				}
			}
			break;
		}
		case "use_button":
			base.TargetTransform = _inventoryGroup.GetUseButtonTransform();
			base.CurrentParameter.rotate = 90f;
			break;
		default:
			base.UpdateTargetTransform();
			break;
		}
	}
}

using MenuData;
using UnityEngine;

namespace PlayGuide;

public class ClickTargetLocatorCraft : ClickTargetLocator
{
	private readonly LeftMenuListGroup _leftMenuGroup;

	private readonly RecipeSelectorGroup _recipeSelectorGroup;

	private readonly ItemCraftingGroup _itemcraftingGroup;

	private readonly BuildGroup _buildGroup;

	private string _targetCategory;

	private string _targetRecipe;

	public ClickTargetLocatorCraft(bool craft = true)
	{
		if (craft)
		{
			_itemcraftingGroup = UIManager.FindScript<ItemCraftingGroup>();
		}
		else
		{
			_buildGroup = UIManager.FindScript<BuildGroup>();
		}
		_recipeSelectorGroup = UIManager.FindScript<RecipeSelectorGroup>();
		_leftMenuGroup = UIManager.FindScript<LeftMenuListGroup>();
	}

	protected override void OnInitialized()
	{
		ClickTargetData clickTargetData = ClickTargetDict.Get("select_category");
		if (clickTargetData != null)
		{
			_targetCategory = clickTargetData.id;
		}
		ClickTargetData clickTargetData2 = ClickTargetDict.Get("select_recipe");
		if (clickTargetData2 != null)
		{
			_targetRecipe = clickTargetData2.id;
		}
	}

	protected override string SelectPhase()
	{
		if ((Object)(object)_itemcraftingGroup != (Object)null && _itemcraftingGroup.IsOpen)
		{
			CraftSlotContainer.CraftState state = GameSystem<ItemCraftingSystem>.Instance().SlotContainer.State;
			if (state == CraftSlotContainer.CraftState.ReadyToCraft)
			{
				return "craft_button";
			}
			return (!((Object)(object)_itemcraftingGroup.GetNextRecipeSlotTransfrom() != (Object)null)) ? "select_item" : "select_slot";
		}
		if ((Object)(object)_buildGroup != (Object)null && _buildGroup.IsOpen)
		{
			BuildSlotContainer.BuildState state2 = GameSystem<BuildSystem>.Instance().SlotContainer.State;
			if (state2 == BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild || state2 == BuildSlotContainer.BuildState.ReadyToBuild)
			{
				return "craft_button";
			}
			return (!((Object)(object)_buildGroup.GetNextRecipeSlotTransfrom() != (Object)null)) ? "select_item" : "select_slot";
		}
		if ((Object)(object)_recipeSelectorGroup != (Object)null && _recipeSelectorGroup.IsOpen)
		{
			if (_recipeSelectorGroup.SelectedRecipe == _targetRecipe)
			{
				return "craft_begin";
			}
			if (_recipeSelectorGroup.SelectedCategory == _targetCategory)
			{
				if (base.CurrentPhase != "select_recipe")
				{
					_recipeSelectorGroup.ScrollToRecipe(RecipeSystem.RecipeType.Crafting, _targetRecipe);
				}
				return "select_recipe";
			}
			return "select_category";
		}
		if ((Object)(object)_leftMenuGroup != (Object)null && _leftMenuGroup.IsMenuVisible())
		{
			return "craft_menu";
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
		case "craft_menu":
			base.TargetTransform = _leftMenuGroup.GetMenuTransform(MenuType.Craft);
			CurrentClickTarget.x = 0.02f;
			CurrentClickTarget.y = -0.01f;
			break;
		case "select_category":
			base.TargetTransform = _recipeSelectorGroup.FindCategoryTransform(_targetCategory);
			break;
		case "select_recipe":
			base.TargetTransform = _recipeSelectorGroup.FindRecipeTransform(_targetRecipe);
			break;
		case "craft_begin":
			base.TargetTransform = _recipeSelectorGroup.GetCraftButtonTransform();
			break;
		case "select_item":
			base.TargetTransform = ((!((Object)(object)_itemcraftingGroup != (Object)null)) ? _buildGroup.GetSelectableItemTranform() : _itemcraftingGroup.GetSelectableItemTranform());
			break;
		case "select_slot":
			base.TargetTransform = ((!((Object)(object)_itemcraftingGroup != (Object)null)) ? _buildGroup.GetNextRecipeSlotTransfrom() : _itemcraftingGroup.GetNextRecipeSlotTransfrom());
			break;
		case "craft_button":
			base.TargetTransform = ((!((Object)(object)_itemcraftingGroup != (Object)null)) ? _buildGroup.GetBuildButtonTransform() : _itemcraftingGroup.GetCraftButtonTransform());
			break;
		}
	}
}

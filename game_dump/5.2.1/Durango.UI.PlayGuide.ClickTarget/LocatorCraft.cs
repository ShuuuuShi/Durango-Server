using Durango.Logic;
using Durango.Logic.PlayGuide;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorCraft : LocatorMenu
{
	private RecipeSelectorGroup _recipeSelectorGroup;

	private CraftGroupBase _craftGroup;

	private string _targetCategory;

	private string _targetRecipe;

	private readonly bool _isCraft;

	private readonly bool _isTutorial;

	public LocatorCraft(bool craft = true, bool tutorial = false)
	{
		_isCraft = craft;
		_isTutorial = tutorial;
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_craftGroup = UIManager.FindScript<CraftGroupBase>();
		_recipeSelectorGroup = UIManager.FindScript<RecipeSelectorGroup>();
		Parameter parameter = Parameters.Get("select_category");
		if (parameter != null)
		{
			_targetCategory = parameter.id;
		}
		Parameter parameter2 = Parameters.Get("select_recipe");
		if (parameter2 != null)
		{
			_targetRecipe = parameter2.id;
		}
		SetMenuType(MenuType.Craft);
	}

	protected override string SelectPhase()
	{
		if (_craftGroup != null && _craftGroup.IsOpened)
		{
			CraftSlotContainer craftSlotContainer = _craftGroup.GetSlotContainer() as CraftSlotContainer;
			if (_isCraft && craftSlotContainer != null && craftSlotContainer.State == CraftSlotContainer.CraftState.ReadyToCraft)
			{
				return "craft_button";
			}
			BuildSlotContainer buildSlotContainer = _craftGroup.GetSlotContainer() as BuildSlotContainer;
			if (!_isCraft && buildSlotContainer != null)
			{
				BuildSlotContainer.BuildState state = buildSlotContainer.State;
				if (state == BuildSlotContainer.BuildState.ReadyToPutMaterialsAndBuild || state == BuildSlotContainer.BuildState.ReadyToBuild || (_isTutorial && IsReady(buildSlotContainer)))
				{
					return "craft_button";
				}
			}
			if (_craftGroup.GetNextRecipeSlotTransfrom() != null)
			{
				return "select_slot";
			}
			return "select_item";
		}
		if (_recipeSelectorGroup != null && _recipeSelectorGroup.IsOpened)
		{
			if (_recipeSelectorGroup.SelectedRecipeId == _targetRecipe)
			{
				return "craft_begin";
			}
			if (_recipeSelectorGroup.SelectedCategoryId == _targetCategory)
			{
				if (base.CurrentPhase != "select_recipe")
				{
					_recipeSelectorGroup.ScrollToRecipe(RecipeSystem.RecipeType.Crafting, _targetRecipe);
				}
				return "select_recipe";
			}
			return "select_category";
		}
		return base.SelectPhase();
	}

	public bool IsReady(SlotContainer slots)
	{
		int slotCount = slots.SlotCount;
		for (int i = 0; i < slotCount; i++)
		{
			if (slots.GetSlotInfo(i).State != SlotInfo.SlotState.FullSelected)
			{
				return false;
			}
		}
		return true;
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "select_category":
			base.TargetTransform = _recipeSelectorGroup.FindCategoryTransform(_targetCategory);
			break;
		case "select_recipe":
			base.TargetTransform = _recipeSelectorGroup.FindRecipeTransform(_targetRecipe);
			break;
		case "craft_begin":
			base.TargetTransform = _recipeSelectorGroup.GetCraftButtonTransform();
			base.CurrentParameter.rotate = 90f;
			break;
		case "select_item":
			base.TargetTransform = _craftGroup.GetSelectableItemTranform();
			break;
		case "select_slot":
			base.TargetTransform = _craftGroup.GetNextRecipeSlotTransfrom();
			break;
		case "craft_button":
			base.TargetTransform = _craftGroup.GetButtonTransform();
			base.CurrentParameter.rotate = 90f;
			break;
		default:
			base.UpdateTargetTransform();
			break;
		}
	}
}

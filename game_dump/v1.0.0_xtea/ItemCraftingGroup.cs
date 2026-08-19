using Crafting;
using L10N;
using Messages;
using UnityEngine;

public class ItemCraftingGroup : UIBase
{
	[SerializeField]
	private RecipeStepSelectWidget _recipeStepSelectWidget;

	[SerializeField]
	private MaterialSelectWidget _materialSelectWidget;

	[SerializeField]
	private CraftExpectResultWidget _craftEstimateResultWidget;

	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private DefaultSelectableButton _buttonCraft;

	private void Awake()
	{
		OnClose();
	}

	private void Start()
	{
		base.OnOpenSucceed += ItemCraftingGroup_OnOpenSucceed;
		_materialSelectWidget.ItemSelectionUpdated += MaterialSelectWidget_ItemSelectionUpdated;
		_titleWidget.OnBack += base.ForceClose;
		_titleWidget.OnClose += UIBase.CloseAllUI;
		UIEventListener.Get(((Component)_buttonCraft).gameObject).onClick = ButtonCraft_OnClick;
	}

	private void OnEnable()
	{
		ItemCraftingSystem itemCraftingSystem = GameSystem<ItemCraftingSystem>.Instance();
		itemCraftingSystem.CraftStartedOnWorkbench += System_CraftStartedOnWorkbench;
		itemCraftingSystem.CraftFailed += CraftFailed;
		itemCraftingSystem.SlotContainer.SlotChanged += SlotContainer_SlotChanged;
		itemCraftingSystem.SlotContainer.ExpectedResultUpdated += SlotContainer_ExpectedResultUpdated;
	}

	private void OnDisable()
	{
		ItemCraftingSystem itemCraftingSystem = GameSystem<ItemCraftingSystem>.Instance();
		itemCraftingSystem.CraftStartedOnWorkbench -= System_CraftStartedOnWorkbench;
		itemCraftingSystem.CraftFailed -= CraftFailed;
		itemCraftingSystem.SlotContainer.SlotChanged -= SlotContainer_SlotChanged;
		itemCraftingSystem.SlotContainer.ExpectedResultUpdated -= SlotContainer_ExpectedResultUpdated;
	}

	protected override bool OnOpen()
	{
		bool isOpen = UIManager.FindScript<RecipeSelectorGroup>().IsOpen;
		_titleWidget.ShowBackButton(isOpen, instant: true);
		base.SoftOpen = !isOpen;
		return base.OnOpen();
	}

	public bool Open(Recipe recipe, Artifact workbench, bool quickFill = false)
	{
		if (recipe.WorkbenchRequired && (Object)(object)workbench == (Object)null)
		{
			return false;
		}
		CraftSlotContainer slotContainer = GameSystem<ItemCraftingSystem>.Instance().SlotContainer;
		slotContainer.Set(recipe, workbench, GameSystem<InventorySystem>.Instance().PlayerInventory);
		_recipeStepSelectWidget.Set(slotContainer);
		_materialSelectWidget.Set(slotContainer);
		if (quickFill)
		{
			slotContainer.QuickFill();
		}
		Open();
		RefreshAll();
		return true;
	}

	private void CraftFailed(string recipeId, ActionInfo actionInfo)
	{
		KSingleton<PlayerController>.Instance().Motion("Craft_Fail");
	}

	public Transform GetSelectableItemTranform()
	{
		ItemIcon2 firstSelectableEnabledItemOrNull = _materialSelectWidget.GetFirstSelectableEnabledItemOrNull();
		return (!((Object)(object)firstSelectableEnabledItemOrNull != (Object)null)) ? null : ((Component)firstSelectableEnabledItemOrNull).transform;
	}

	public Transform GetNextRecipeSlotTransfrom()
	{
		RecipeSlotWidget nextRecipeSlotWidget = _recipeStepSelectWidget.GetNextRecipeSlotWidget();
		return (!((Object)(object)nextRecipeSlotWidget != (Object)null)) ? null : ((Component)nextRecipeSlotWidget).transform;
	}

	public Transform GetCraftButtonTransform()
	{
		return ((Component)_buttonCraft).transform;
	}

	private void RefreshAll()
	{
		_recipeStepSelectWidget.Refresh();
		_materialSelectWidget.Refresh();
		_craftEstimateResultWidget.Refresh();
		RefreshCraftButton();
	}

	private void RefreshCraftButton()
	{
		CraftSlotContainer slotContainer = GameSystem<ItemCraftingSystem>.Instance().SlotContainer;
		if (slotContainer.State == CraftSlotContainer.CraftState.CanQuickFill)
		{
			_buttonCraft.Text = T._("자동 채우기");
			_buttonCraft.Disable = false;
		}
		else
		{
			_buttonCraft.Text = T._("제작");
			_buttonCraft.Disable = slotContainer.State != CraftSlotContainer.CraftState.ReadyToCraft;
		}
	}

	private void MaterialSelectWidget_ItemSelectionUpdated()
	{
		ItemCraftingSystem itemCraftingSystem = GameSystem<ItemCraftingSystem>.Instance();
		CraftSlotContainer slotContainer = itemCraftingSystem.SlotContainer;
		itemCraftingSystem.RequestEstimateResult();
		if (slotContainer.CurrentSlot != null)
		{
			_recipeStepSelectWidget.RefreshSlot(slotContainer.CurrentSlot.Index);
		}
		_recipeStepSelectWidget.RefreshProgressPercentage();
		RefreshCraftButton();
	}

	private void ItemCraftingGroup_OnOpenSucceed()
	{
		_materialSelectWidget.RepositionItemList();
	}

	private void ButtonCraft_OnClick(GameObject go)
	{
		ItemCraftingSystem system = GameSystem<ItemCraftingSystem>.Instance();
		CraftSlotContainer slotContainer = system.SlotContainer;
		switch (slotContainer.State)
		{
		case CraftSlotContainer.CraftState.CanQuickFill:
			slotContainer.QuickFill();
			system.RequestEstimateResult();
			RefreshAll();
			break;
		case CraftSlotContainer.CraftState.ToolNotReady:
			UIManager.SystemMsg(T._("도구가 선택되지 않았습니다"));
			break;
		case CraftSlotContainer.CraftState.MaterialsNotReady:
			UIManager.SystemMsg(T._("재료가 선택되지 않았습니다"));
			break;
		case CraftSlotContainer.CraftState.ReadyToCraft:
			if (slotContainer.HasLockedItem())
			{
				UIManager.MessageBox.Show(T._("보호 중인 아이템이 포함되어 있습니다. 정말로 제작하시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						system.Craft();
						UIBase.CloseAllUI();
					}
				});
			}
			else
			{
				system.Craft();
				UIBase.CloseAllUI();
			}
			break;
		}
	}

	private void SlotContainer_SlotChanged(int previousIndex)
	{
		CraftSlotContainer slotContainer = GameSystem<ItemCraftingSystem>.Instance().SlotContainer;
		_recipeStepSelectWidget.RefreshSlot(previousIndex);
		if (slotContainer.CurrentSlot != null && slotContainer.CurrentSlot.Index != previousIndex)
		{
			_recipeStepSelectWidget.RefreshSlot(slotContainer.CurrentSlot.Index);
		}
		_materialSelectWidget.Refresh();
	}

	private void SlotContainer_ExpectedResultUpdated(SlotContainer slotContainer)
	{
		_craftEstimateResultWidget.Refresh();
	}

	private void System_CraftStartedOnWorkbench()
	{
		UIManager.SystemMsg(T._("제작을 시작하였다"));
	}
}

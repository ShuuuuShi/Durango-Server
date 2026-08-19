using System;
using Crafting;
using ItemSystem;
using Shared.Item;
using Shared.System;
using UnityEngine;

public class DyeGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private ItemList _targetItemList;

	[SerializeField]
	private ItemList _dyeItemList;

	[SerializeField]
	private DyePartsWidget _dyePartsWidget;

	[SerializeField]
	private DyeResultWidget _dyeResultWidget;

	[SerializeField]
	private DefaultSelectableButton _applyButton;

	private Artifact _workbench;

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		_titleWidget.OnBack += Close;
		ItemList targetItemList = _targetItemList;
		targetItemList.OnUpdateSelectItem = (Action)Delegate.Combine(targetItemList.OnUpdateSelectItem, new Action(OnUpdateTargetItem));
		ItemList dyeItemList = _dyeItemList;
		dyeItemList.OnUpdateSelectItem = (Action)Delegate.Combine(dyeItemList.OnUpdateSelectItem, new Action(OnUpdateDyeItem));
		_dyePartsWidget.SelectPartChanged += RefreshDyeResult;
		DefaultSelectableButton applyButton = _applyButton;
		applyButton.Clicked = (Action)Delegate.Combine(applyButton.Clicked, new Action(OnDyeApply));
		base.OnOpenSucceed += delegate
		{
			_targetItemList.ClearSelectItem(sendEvent: false);
			_dyeItemList.ClearSelectItem(sendEvent: false);
			OnUpdateInventory();
			OnUpdateTargetItem();
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
			GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		};
		base.OnCloseSucceed += delegate
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
		};
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Dye, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!((Object)(object)targetComponent == (Object)null))
			{
				Open(targetComponent);
			}
		});
		base.OnClose();
	}

	public void Open(Artifact workbench)
	{
		_workbench = workbench;
		Open();
	}

	protected override bool OnClose()
	{
		_workbench = null;
		return base.OnClose();
	}

	private void OnUpdateInventory()
	{
		if (base.IsOpen)
		{
			_targetItemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.ModifiableCount > 0 && data.IsDyeable());
			_dyeItemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.HasTag("dye_medicine") || data.HasTag("decolorizer"));
		}
	}

	private void OnUpdateTargetItem()
	{
		ItemData itemData = ((_targetItemList.SelectedItemList.Count != 0) ? _targetItemList.SelectedItemList[_targetItemList.SelectedItemList.Count - 1].Item : null);
		if (itemData != null)
		{
			_dyePartsWidget.Set(itemData);
			_dyeResultWidget.SetModel(itemData);
		}
		RefreshDyeResult();
	}

	private void OnUpdateDyeItem()
	{
		RefreshDyeResult();
	}

	private void RefreshDyeResult()
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		ItemData itemData = ((_targetItemList.SelectedItemList.Count != 0) ? _targetItemList.SelectedItemList[_targetItemList.SelectedItemList.Count - 1].Item : null);
		if (itemData == null)
		{
			_dyePartsWidget.Reset();
			_dyeResultWidget.SetUnknownModel();
			return;
		}
		ItemColor colors = itemData.Colors;
		ItemData itemData2 = ((_dyeItemList.SelectedItemList.Count != 0) ? _dyeItemList.SelectedItemList[_dyeItemList.SelectedItemList.Count - 1].Item : null);
		if (itemData2 == null)
		{
			_dyeResultWidget.ResetEstimate();
		}
		else
		{
			int selectedPart = _dyePartsWidget.SelectedPart;
			ColorChannel channel = (ColorChannel)selectedPart;
			if (itemData2.HasTag("decolorizer"))
			{
				colors.Bleaching(selectedPart, (GameSystem<RecipeSystem>.Instance().GetBleachingRecipe(channel) as RecipeModify)?.AddColorRate ?? 0f);
			}
			else
			{
				RecipeModify recipeModify = GameSystem<RecipeSystem>.Instance().GetDyeingRecipe(channel) as RecipeModify;
				colors.Dyeing(selectedPart, itemData2.Colors[0], recipeModify?.AddColorRate ?? 0f);
			}
			ItemCraftingSystem.DyeingEstimate(_workbench, itemData, itemData2, channel, _dyeResultWidget.SetEstimate);
		}
		_dyeResultWidget.SetColor(colors);
	}

	private void OnDyeApply()
	{
		ItemData lastClickedItemData = _targetItemList.LastClickedItemData;
		ItemData lastClickedItemData2 = _dyeItemList.LastClickedItemData;
		if (lastClickedItemData != null && lastClickedItemData2 != null)
		{
			ColorChannel selectedPart = (ColorChannel)_dyePartsWidget.SelectedPart;
			GameSystem<ItemCraftingSystem>.Instance().Dyeing(_workbench, lastClickedItemData, lastClickedItemData2, selectedPart);
			ForceClose();
		}
	}
}

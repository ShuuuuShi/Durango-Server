using System;
using Crafting;
using Durango.Logic.Item;
using Durango.UI.Control;
using InteractionData;
using L10N;
using NestedPrefab;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

public class DyeGroup : UIBase
{
	private enum Mode
	{
		Dyeing,
		Bleaching
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private UILabel _dyesListTitle;

	[SerializeField]
	private UILabel _dyeTargetTitle;

	[SerializeField]
	private NestedPrefabLinker _targetItemListLinker;

	[SerializeField]
	private NestedPrefabLinker _dyeItemListLinker;

	[SerializeField]
	private DyePartsWidget _dyePartsWidget;

	[SerializeField]
	private DyeResultWidget _dyeResultWidget;

	[SerializeField]
	private SelectableButton _applyButton;

	[SerializeField]
	private GameObject _bleachGuideMsgObj;

	private Artifact _workbench;

	private ItemList _targetItemList;

	private ItemList _dyeItemList;

	private Mode _currentMode;

	private void Start()
	{
		_targetItemList = _targetItemListLinker.Object.GetComponent<ItemList>();
		_dyeItemList = _dyeItemListLinker.Object.GetComponent<ItemList>();
		ItemList targetItemList = _targetItemList;
		targetItemList.OnUpdateSelectItem = (Action)Delegate.Combine(targetItemList.OnUpdateSelectItem, new Action(OnUpdateTargetItem));
		ItemList dyeItemList = _dyeItemList;
		dyeItemList.OnUpdateSelectItem = (Action)Delegate.Combine(dyeItemList.OnUpdateSelectItem, new Action(OnUpdateDyeItem));
		_dyePartsWidget.SelectPartChanged += RefreshDyeResult;
		SelectableButton applyButton = _applyButton;
		applyButton.Clicked = (Action)Delegate.Combine(applyButton.Clicked, new Action(OnDyeApply));
		base.OnOpenSucceed += delegate
		{
			_targetItemList.DeselectAllItems(sendEvent: false);
			_dyeItemList.DeselectAllItems(sendEvent: false);
			OnUpdateInventory();
			OnUpdateTargetItem();
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
		};
		base.OnCloseSucceed += delegate
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
		};
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Dye, delegate(InteractionObject target)
		{
			Artifact targetComponent2 = target.GetTargetComponent<Artifact>();
			if (!(targetComponent2 == null))
			{
				Open(targetComponent2, Mode.Dyeing);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Bleach, delegate(InteractionObject target)
		{
			Artifact targetComponent = target.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				Open(targetComponent, Mode.Bleaching);
			}
		});
		SetChildrenActive(activated: false);
	}

	private void Open(Artifact workbench, Mode mode)
	{
		_currentMode = mode;
		_workbench = workbench;
		switch (mode)
		{
		case Mode.Dyeing:
			_titleWidget.Object.SetTitle(T._("염색"));
			_dyesListTitle.text = T._("염색약");
			_dyeTargetTitle.text = T._("염색 부위");
			_bleachGuideMsgObj.SetActive(value: false);
			_applyButton.Text = T._("염색하기");
			break;
		case Mode.Bleaching:
			_titleWidget.Object.SetTitle(T._("탈색"));
			_dyesListTitle.text = T._("탈색제");
			_dyeTargetTitle.text = T._("탈색 부위");
			_bleachGuideMsgObj.SetActive(value: true);
			_applyButton.Text = T._("탈색하기");
			break;
		default:
			Debug.LogError(string.Concat(typeof(DyeGroup), "mode not found"));
			break;
		}
		Open();
	}

	protected override bool TryClose()
	{
		_workbench = null;
		return base.TryClose();
	}

	private void OnUpdateInventory()
	{
		if (!base.IsOpened)
		{
			return;
		}
		switch (_currentMode)
		{
		case Mode.Dyeing:
			_targetItemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.IsDyeable() && !data.IsEquipments);
			_dyeItemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.HasTag("dye_medicine"));
			break;
		case Mode.Bleaching:
			_targetItemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.ModifiableCount > 0 && data.IsDyeable() && !data.IsEquipments);
			_dyeItemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData data) => data.HasTag("decolorizer"));
			break;
		default:
			Debug.LogError(string.Concat(typeof(DyeGroup), "mode not found"));
			break;
		}
	}

	private void OnUpdateTargetItem()
	{
		ItemData lastSelectedItem = _targetItemList.LastSelectedItem;
		if (lastSelectedItem != null)
		{
			_dyePartsWidget.Set(lastSelectedItem);
			_dyeResultWidget.SetModel(lastSelectedItem);
		}
		RefreshDyeResult();
	}

	private void OnUpdateDyeItem()
	{
		RefreshDyeResult();
	}

	private void RefreshDyeResult()
	{
		ItemData lastSelectedItem = _targetItemList.LastSelectedItem;
		_applyButton.Disabled = lastSelectedItem == null || _dyeItemList.LastSelectedItem == null;
		if (lastSelectedItem == null)
		{
			_dyePartsWidget.Reset();
			_dyeResultWidget.SetUnknownModel();
			return;
		}
		ItemColor colors = lastSelectedItem.Colors;
		ItemData lastSelectedItem2 = _dyeItemList.LastSelectedItem;
		if (lastSelectedItem2 == null)
		{
			_dyeResultWidget.ResetEstimate();
		}
		else
		{
			int selectedPart = _dyePartsWidget.SelectedPart;
			ColorChannel channel = (ColorChannel)selectedPart;
			if (lastSelectedItem2.HasTag("decolorizer"))
			{
				colors.Bleaching(selectedPart, (GameSystem<RecipeSystem>.Instance().GetBleachingRecipe(channel) as RecipeModify)?.AddColorRate ?? 0f);
			}
			else
			{
				RecipeModify recipeModify = GameSystem<RecipeSystem>.Instance().GetDyeingRecipe(channel) as RecipeModify;
				colors.Dyeing(selectedPart, lastSelectedItem2.Colors[0], recipeModify?.AddColorRate ?? 0f);
			}
			CraftSystem.DyeingEstimate(_workbench, lastSelectedItem, lastSelectedItem2, channel, _dyeResultWidget.SetEstimate);
		}
		_dyeResultWidget.SetColor(colors);
	}

	private void OnDyeApply()
	{
		ItemData item = _targetItemList.LastClickedItem;
		ItemData dye = _dyeItemList.LastClickedItem;
		if (item != null && dye != null)
		{
			ColorChannel channel = (ColorChannel)_dyePartsWidget.SelectedPart;
			Artifact workbench = _workbench;
			ItemData[] items = new ItemData[2] { item, dye };
			UIManager.MessageBox.ShowLockConfirm(items, delegate
			{
				DyeItem(workbench, item, dye, channel);
			});
			ForceClose();
		}
	}

	public static void DyeItem(Artifact workbench, ItemData item, ItemData dye, ColorChannel channel)
	{
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
			{
				GameSystem<CraftSystem>.Instance().Dyeing(workbench, item, dye, channel);
			});
		}
		else
		{
			GameSystem<CraftSystem>.Instance().Dyeing(workbench, item, dye, channel);
		}
	}
}

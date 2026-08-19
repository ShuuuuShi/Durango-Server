using System;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.UI.Popup;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class RepairGroup : UIBase
{
	private enum Type
	{
		Item,
		Artifact
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private RepairKitsWidget _repairKitsWidget;

	[SerializeField]
	private WarpGemRepairWidget _warpGemRepairWidget;

	[SerializeField]
	private RepairResultWidget _repairResultWidget;

	[SerializeField]
	private SelectableButton _applyButton;

	private Type? _type;

	private ItemData _targetItem;

	private Artifact _targetArtifact;

	private float _refreshDurabilityAt;

	private Gauge Durability
	{
		get
		{
			Type? type = _type;
			if (type.HasValue)
			{
				switch (type.Value)
				{
				case Type.Item:
					return _targetItem.Durability;
				case Type.Artifact:
					return _targetArtifact.Durability;
				}
			}
			return null;
		}
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("수리"));
		_repairKitsWidget.Init();
		_warpGemRepairWidget.Init();
		_repairResultWidget.Init();
		_repairKitsWidget.RepairValueChanged += RepairKitsWidget_RepairValueChanged;
		_repairKitsWidget.JumpToRecipeUIButtonClicked += RepairKitsWidget_JumpToRecipeUIButtonClicked;
		_repairKitsWidget.JumpToMarketUIButtonClicked += RepairKitsWidget_JumpToMarketUIButtonClicked;
		_warpGemRepairWidget.RadioButtonStateChanged += WarpGemRepairWidget_RadioButtonStateChanged;
		SelectableButton applyButton = _applyButton;
		applyButton.Clicked = (Action)Delegate.Combine(applyButton.Clicked, new Action(OnApply));
		base.OnOpenSucceed += delegate
		{
			_repairKitsWidget.RefreshRepairKitItemList();
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += InventorySystem_PlayerInventoryUpdated;
		};
		base.OnCloseSucceed += delegate
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= InventorySystem_PlayerInventoryUpdated;
		};
		GameSystem<InteractionSystem>.Instance().PostTouched += OnPostTouched;
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RepairArtifact, delegate(InteractionObject target)
		{
			Open(target.GetTargetComponent<Artifact>());
		});
		TryClose();
	}

	private void Update()
	{
		if (_refreshDurabilityAt > 0f && _refreshDurabilityAt < Time.time)
		{
			RefreshDurability();
		}
	}

	private void OnPostTouched(InteractionMenuList menuList, InteractionObject obj)
	{
		if (menuList.IndexOf(Interaction.RepairArtifact) != -1)
		{
			InteractionMenuData data = new InteractionMenuData(Interaction.RepairArtifactImmediately);
			Artifact targetComponent = obj.GetTargetComponent<Artifact>();
			if (targetComponent == null)
			{
				data.Name = T._("즉시 수리");
			}
			else
			{
				int repairRequirementPerformance = Singleton<Constants>.Instance.Repair.GetRepairRequirementPerformance(targetComponent.Blueprint.RepairRequirement, targetComponent.ArtifactState.Level);
				data.Name = string.Format("{0} {1}", T._("즉시 수리"), Inventory.CurrencyFormat(repairRequirementPerformance, Currency.Gem));
			}
			menuList.Add(data);
		}
	}

	public override bool Open()
	{
		return false;
	}

	public void Open(ItemData itemData)
	{
		if (!itemData.IsRepairable)
		{
			return;
		}
		float limitDurability = Singleton<Constants>.Instance.Repair.Item.LimitDurability;
		if (itemData.Durability.Max() < limitDurability)
		{
			UIManager.MessageBox.Show(T._("해당 아이템은 <em>전체 내구도가 {0} 미만</em>으로 떨어져,\n더 이상 수리할 수 없습니다.", limitDurability));
			return;
		}
		_type = Type.Item;
		_targetItem = itemData;
		_targetArtifact = null;
		if (itemData.RepairRequirement.HasValue)
		{
			_repairKitsWidget.Refresh(itemData.RepairRequirement.Value);
			_warpGemRepairWidget.Refresh(itemData.RepairRequirement.Value);
		}
		_repairResultWidget.Refresh(itemData);
		RefreshDurability();
		base.Open();
	}

	private void Open(Artifact artifact)
	{
		if (!(artifact == null) && artifact.Blueprint != null && artifact.Blueprint.RepairRequirement != 0 && artifact.Durability != null)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			float limitDurability = Singleton<Constants>.Instance.Repair.Item.LimitDurability;
			if (artifact.Durability.Max(predictedServerTime) < limitDurability)
			{
				UIManager.MessageBox.Show(T._("해당 건축물은 <em>전체 내구도가 {0} 미만</em>으로 떨어져,\n더 이상 수리할 수 없습니다.", limitDurability));
				return;
			}
			_type = Type.Artifact;
			_targetItem = null;
			_targetArtifact = artifact;
			_repairKitsWidget.Refresh(artifact);
			_warpGemRepairWidget.Refresh(artifact);
			_repairResultWidget.Refresh(artifact);
			RefreshDurability();
			base.Open();
		}
	}

	private void RefreshDurability()
	{
		Type? type = _type;
		if (type.HasValue)
		{
			switch (type.Value)
			{
			case Type.Item:
				_repairResultWidget.RefreshDurability(Durability, isArtifact: false);
				break;
			case Type.Artifact:
				_repairResultWidget.RefreshDurability(Durability, isArtifact: true);
				break;
			}
		}
		Gauge durability = Durability;
		_refreshDurabilityAt = ((durability == null || !(durability.Get() > 0f)) ? 0f : (Time.time + 60f));
	}

	private void RefreshButtonAndResultWidget()
	{
		bool flag = !_repairKitsWidget.IsInsufficient || _warpGemRepairWidget.IsChecked;
		_applyButton.Disabled = !flag;
		_repairResultWidget.ShowResult = flag;
	}

	private void ApplyRepair([CanBeNull] string[] kitItemIds)
	{
		if (PlayerBehavior.LocalPlayer.IsRiding)
		{
			VehicleBase.RequestUnmountIfRiding(immediately: true, delegate
			{
				SendRepairMessage(kitItemIds);
			});
		}
		else
		{
			SendRepairMessage(kitItemIds);
		}
		Type? type = _type;
		if (type.HasValue)
		{
			switch (type.Value)
			{
			case Type.Item:
				_applyButton.Disabled = true;
				_applyButton.ShowLoadingRing(show: true);
				break;
			case Type.Artifact:
				UIBase.CloseAllUI();
				ShowLoadingRingToArtifact();
				break;
			}
		}
	}

	private void SendRepairMessage([CanBeNull] string[] kitItemIds)
	{
		Type? type = _type;
		if (type.HasValue)
		{
			switch (type.Value)
			{
			case Type.Item:
				RepairSystem.RepairItem(_targetItem.Id, kitItemIds, OnItemRepair);
				break;
			case Type.Artifact:
				RepairSystem.RepairArtifact(_targetArtifact.EntityId, _targetArtifact.WorldTile, kitItemIds, OnArtifactRepair);
				break;
			}
		}
	}

	private void ShowLoadingRingToArtifact()
	{
		Vector2 centerTile = _targetArtifact.CenterTile;
		UIManager.Popup.LoadingRing.AttachToClientPosition(Durango.Terrain.Util.TilePositionToClientPosition(centerTile));
	}

	private void HideLoadingRingFromArtifact()
	{
		LoadingRingWidget loadingRing = UIManager.Popup.LoadingRing;
		if (loadingRing.AttachMode == LoadingRingWidget.Mode.ClientPosition)
		{
			loadingRing.Hide();
		}
	}

	private void RepairKitsWidget_RepairValueChanged()
	{
		if (_repairKitsWidget.SelectedItems.Count > 0)
		{
			_warpGemRepairWidget.IsChecked = false;
		}
		RefreshButtonAndResultWidget();
	}

	private void RepairKitsWidget_JumpToRecipeUIButtonClicked(string recipeId)
	{
		UIBase.CloseAllUI();
		RecipeSelectorGroup.OpenRecipeOrLearnableUI(RecipeSystem.RecipeType.Crafting, recipeId);
	}

	private void RepairKitsWidget_JumpToMarketUIButtonClicked(string tagId)
	{
		UIBase.CloseAllUI();
		UIManager.FindScript<MarketGroup>().OpenAndSearch(new OrTagFilter(tagId, 0), null);
	}

	private void WarpGemRepairWidget_RadioButtonStateChanged()
	{
		if (_warpGemRepairWidget.IsChecked)
		{
			_repairKitsWidget.ClearSelectedItems();
		}
		RefreshButtonAndResultWidget();
	}

	private void OnApply()
	{
		Type? type = _type;
		if (!type.HasValue)
		{
			return;
		}
		switch (type.Value)
		{
		case Type.Item:
			ApplyRepair((!_warpGemRepairWidget.IsChecked) ? Durango.Logic.Item.Util.ItemsToIds(_repairKitsWidget.SelectedItems) : null);
			break;
		case Type.Artifact:
			if (_warpGemRepairWidget.IsChecked)
			{
				ApplyRepair(null);
				break;
			}
			UIManager.MessageBox.Show(T._("수리하시겠습니까?"), T._("[icon=icon_make_alert] 수리 중 공격을 받으면 건축물의 수리가 취소되고, 사용된 수리키트는 회수할 수 없습니다."), delegate(bool ok)
			{
				if (ok)
				{
					ApplyRepair(Durango.Logic.Item.Util.ItemsToIds(_repairKitsWidget.SelectedItems));
				}
			});
			break;
		}
	}

	private void InventorySystem_PlayerInventoryUpdated()
	{
		if (base.IsOpened)
		{
			_repairKitsWidget.RefreshRepairKitItemList();
		}
	}

	private void OnItemRepair(bool success)
	{
		_applyButton.ShowLoadingRing(show: false);
		if (success)
		{
			UIBase.CloseAllUI();
		}
		else
		{
			RefreshButtonAndResultWidget();
		}
	}

	private void OnArtifactRepair(bool success)
	{
		HideLoadingRingFromArtifact();
	}
}

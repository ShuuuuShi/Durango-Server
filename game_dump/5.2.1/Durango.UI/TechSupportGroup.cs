using Crafting;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

[Uri("TechSupport")]
public class TechSupportGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private RectLayoutComponent _rectLayout;

	[SerializeField]
	private GameObject _itemNotFound;

	[SerializeField]
	private TechSupportEstimatePageWidget _estimatePageWidget;

	[SerializeField]
	private FlavorTextWidget _portraitWidget;

	[SerializeField]
	private NestedPrefabLinker _itemListPrefabLinker;

	private ItemList _itemList;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Craft;
		_titleWidget.Object.SetTitle(T._("기술지원"));
		InitializeItemList();
		base.OnOpenSucceed += delegate
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += InventorySystem_PlayerInventoryUpdated;
			GameSystem<TechSupportSystem>.Instance().DecorationRemoved += TechSupportSystem_DecorationRemoved;
			GameSystem<TechSupportSystem>.Instance().RequestAllEstimates(_estimatePageWidget.PropKey);
			ClearItem();
			RefreshItemList();
			UpdateLayout();
		};
		base.OnCloseSucceed += delegate
		{
			GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= InventorySystem_PlayerInventoryUpdated;
			GameSystem<TechSupportSystem>.Instance().DecorationRemoved -= TechSupportSystem_DecorationRemoved;
			GameSystem<TechSupportSystem>.Instance().ClearEstimates();
			UIManager.Popup.Tooltip<TechSupportEstimatePopup>().Hide();
		};
		base.TryClose();
	}

	[Uri]
	public void Open(string entityId)
	{
		Artifact artifact = Singleton<ArtifactManager>.Instance().Find(entityId);
		if (!(artifact == null))
		{
			Open(artifact);
		}
	}

	public void Open([NotNull] Artifact artifact)
	{
		_estimatePageWidget.SetArtifact(artifact);
		Open();
	}

	private void InitializeItemList()
	{
		_itemList = _itemListPrefabLinker.Object.GetComponent<ItemList>();
		_itemList.SelectableCount = 1;
		_itemList.FixedIconSize = true;
		_itemList.EquipmentsSelectable = true;
		_itemList.OnUpdateSelectItem = ItemList_OnUpdateSelectItem;
	}

	private void RefreshItemList()
	{
		_itemList.DeselectAllItems(sendEvent: false);
		_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList, TechSupportSystem.CanTechSupport);
		int usableCount = _itemList.UsableCount;
		_itemNotFound.SetActive(usableCount == 0);
		_itemList.gameObject.SetActive(usableCount > 0);
		_itemList.Reposition(resetPosition: false, tween: false);
		_itemList.SelectItem(_estimatePageWidget.Target.Item, sendEvent: true, scrollTo: true);
	}

	private void SetItem(ItemData item)
	{
		_estimatePageWidget.SetItem(new TechSupportTarget(item, 0));
	}

	private void ClearItem()
	{
		_estimatePageWidget.SetItem(default(TechSupportTarget));
	}

	private void UpdateLayout()
	{
		_portraitWidget.gameObject.SetActive(!base.IsPortrait);
		_rectLayout.UpdateLayout();
		UIUtility.UpdateAnchors(_rectLayout.transform);
	}

	private void InventorySystem_PlayerInventoryUpdated()
	{
		if (base.IsOpened)
		{
			RefreshItemList();
		}
	}

	private void TechSupportSystem_DecorationRemoved()
	{
		if (base.IsOpened)
		{
			ClearItem();
			RefreshItemList();
		}
	}

	private void ItemList_OnUpdateSelectItem()
	{
		SetItem(_itemList.LastSelectedItem);
	}
}

using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI.Popup;

public class PopupItemSelector : TooltipBase
{
	private enum Inventory
	{
		My,
		Target
	}

	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private SelectableButton _okBtn;

	[SerializeField]
	private SelectableButton _cancelBtn;

	[SerializeField]
	private KeyValueLabel _titleLabel;

	[SerializeField]
	private UILabel _helperLabel;

	[SerializeField]
	private UIWidget _helperWidget;

	private List<ItemData> _items;

	private Inventory? _inventory;

	private int _selectableCount = 1;

	private Func<int> _selectableCountGetter;

	private Func<ItemData, float> _itemAmountGetter;

	private ItemList _itemList;

	private bool _isTitleOverride;

	private bool _hasHelperText;

	private string _confirmText;

	private string _autoFillText;

	private string _cancelText;

	private ItemInfoTooltip _itemInfoTooltip;

	private Predicate<ItemData> _filterFunc;

	private Util.ItemDelegate _callbackItemFunction;

	private Util.ItemListDelegate _callbackListFunction;

	private Util.ItemDelegate _callbackItemSelectChangedFunction;

	private Util.ItemListDelegate _callbackItemsSelectChangedFunction;

	private bool _isConfirmed;

	private Action _onClickTitle;

	private RectLayoutComponent _layoutComponent;

	private int _baseWidth;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void OnAwake()
	{
		_layoutComponent = GetComponent<RectLayoutComponent>();
		_okBtn.Clicked = OnConfirm;
		_cancelBtn.Clicked = delegate
		{
			Hide();
		};
		_itemList = _itemListLinker.Object.GetComponent<ItemList>();
		_itemList.FixedIconSize = true;
		_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
		_itemList.OnLongPress = _itemList.DefaultLongPress;
		_baseWidth = base.Widget.width;
		UIEventListener uIEventListener = UIEventListener.Get(_titleLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickTitle));
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnPlayerInventoryUpdate;
		GameSystem<InventorySystem>.Instance().TrackingInventoryUpdated += OnTargetInventoryUpdate;
	}

	private void OnPlayerInventoryUpdate()
	{
		Inventory? inventory = _inventory;
		if (inventory.HasValue && _inventory.Value == Inventory.My)
		{
			RefreshItemList();
		}
	}

	private void OnTargetInventoryUpdate()
	{
		Inventory? inventory = _inventory;
		if (inventory.HasValue && _inventory.Value == Inventory.Target)
		{
			RefreshItemList();
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirm();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _okBtn;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _cancelBtn;
	}

	private void OnConfirm()
	{
		if (_itemList.SelectableCount > 0 && _itemList.SelectedList.Count == 0)
		{
			_itemList.SelectAllItems();
			return;
		}
		if (_callbackItemFunction != null)
		{
			_callbackItemFunction(_itemList.LastSelectedItem);
		}
		if (_callbackListFunction != null)
		{
			_callbackListFunction(_itemList.SelectedList);
		}
		_isConfirmed = true;
		Hide();
	}

	private void OnUpdateSelectItem()
	{
		ItemData lastClickedItem = _itemList.LastClickedItem;
		if (_callbackItemSelectChangedFunction != null)
		{
			_callbackItemSelectChangedFunction(lastClickedItem);
		}
		if (_callbackItemsSelectChangedFunction != null)
		{
			_callbackItemsSelectChangedFunction(_itemList.SelectedList);
		}
		RefreshButtonText();
		if (lastClickedItem == null)
		{
			if (_itemInfoTooltip != null)
			{
				_itemInfoTooltip.Hide();
				_itemInfoTooltip = null;
			}
			return;
		}
		ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
		itemInfoTooltip.Sign = 1;
		itemInfoTooltip.Set(lastClickedItem);
		itemInfoTooltip.AutoPosition = false;
		itemInfoTooltip.Show();
		Vector3 pos = base.Widget.GetPosition(1f, 0.5f) + Vector3.right * 10f;
		itemInfoTooltip.Widget.SetPosition(pos, 0f, 0.5f);
		itemInfoTooltip.HideIgnoreParent = base.transform;
		itemInfoTooltip.HideArrow();
		itemInfoTooltip.AddOnFinished(OnHideItemInfoTooltip);
		base.HideIgnoreParent = itemInfoTooltip.transform;
		_itemInfoTooltip = itemInfoTooltip;
	}

	private void OnHideItemInfoTooltip()
	{
		_itemInfoTooltip = null;
	}

	private void OnClickTitle(GameObject obj)
	{
		if (_onClickTitle != null)
		{
			_onClickTitle();
		}
	}

	private void RefreshButtonText()
	{
		if (_itemList.SelectableCount > 0 && _itemList.SelectedList.Count == 0)
		{
			_okBtn.Text = ((!string.IsNullOrEmpty(_autoFillText)) ? _autoFillText : T._("자동 채우기"));
		}
		else
		{
			_okBtn.Text = ((!string.IsNullOrEmpty(_confirmText)) ? _confirmText : T._("확인"));
		}
		_cancelBtn.Text = ((!string.IsNullOrEmpty(_cancelText)) ? _cancelText : T._("취소"));
	}

	public PopupItemSelector Items(List<ItemData> items)
	{
		_items = items;
		_inventory = null;
		return this;
	}

	public PopupItemSelector MyInventory()
	{
		_inventory = Inventory.My;
		return this;
	}

	public PopupItemSelector TargetInventory()
	{
		_inventory = Inventory.Target;
		return this;
	}

	public PopupItemSelector Filter(Predicate<ItemData> filter)
	{
		_filterFunc = filter;
		return this;
	}

	public PopupItemSelector SelectableCount(int count, Func<ItemData, float> itemAmountGetter = null)
	{
		_selectableCount = count;
		_itemAmountGetter = itemAmountGetter;
		return this;
	}

	public PopupItemSelector SelectableCount(Func<int> countGetter, Func<ItemData, float> itemAmountGetter = null)
	{
		_selectableCountGetter = countGetter;
		_itemAmountGetter = itemAmountGetter;
		return this;
	}

	public PopupItemSelector Title(SyncString title)
	{
		return Title(title, null);
	}

	public PopupItemSelector Title(SyncString title, SyncString subTitle)
	{
		_isTitleOverride = true;
		_titleLabel.Set(title, subTitle);
		return this;
	}

	public PopupItemSelector TitleClicked(Action onTitleClick)
	{
		_onClickTitle = onTitleClick;
		return this;
	}

	public PopupItemSelector ConfirmText(string text)
	{
		_confirmText = text;
		return this;
	}

	public PopupItemSelector AutoFillText(string text)
	{
		_autoFillText = text;
		return this;
	}

	public PopupItemSelector CancelText(string text)
	{
		_cancelText = text;
		return this;
	}

	public PopupItemSelector OnConfirmed(Util.ItemDelegate callback)
	{
		_callbackItemFunction = callback;
		return this;
	}

	public PopupItemSelector OnConfirmed(Util.ItemListDelegate callback)
	{
		_callbackListFunction = callback;
		return this;
	}

	public PopupItemSelector OnChanged(Util.ItemDelegate callback)
	{
		_callbackItemSelectChangedFunction = callback;
		return this;
	}

	public PopupItemSelector OnChanged(Util.ItemListDelegate callback)
	{
		_callbackItemsSelectChangedFunction = callback;
		return this;
	}

	public PopupItemSelector HelpText(string text)
	{
		_hasHelperText = true;
		_helperLabel.text = text;
		return this;
	}

	private void RefreshItemList()
	{
		Inventory? inventory = _inventory;
		if (inventory.HasValue)
		{
			switch (inventory.Value)
			{
			case Inventory.My:
				_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerInventory.Items, _filterFunc);
				return;
			case Inventory.Target:
				_itemList.SetItemList(GameSystem<InventorySystem>.Instance().TrackingInventory.Items, _filterFunc);
				return;
			}
		}
		_itemList.SetItemList(_items, _filterFunc);
	}

	protected override void OnShow()
	{
		base.OnShow();
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(UIBase.AnchorType.Base);
		base.Widget.SetPosition(rootAnchor.GetPosition(0f, 0.5f) + Vector3.right * 10f, 0f, 0.5f);
		PlayerController.MotionUpdater.Motion("Avatar_Ransack");
		_isConfirmed = false;
	}

	protected override void FillData()
	{
		if (_items == null)
		{
			Inventory? inventory = _inventory;
			if (!inventory.HasValue)
			{
				_inventory = Inventory.My;
			}
		}
		RefreshItemList();
		_itemList.SelectableCount = _selectableCount;
		_itemList.SetSelectableAmount(_itemAmountGetter, _selectableCountGetter);
		RefreshButtonText();
		if (!_isTitleOverride)
		{
			_titleLabel.Set(T._("아이템 선택 창"), null);
		}
	}

	protected override void UpdateLayout()
	{
		if (_helperWidget.gameObject.SetActiveAnd(_hasHelperText))
		{
			_helperWidget.height = _helperLabel.height + 20;
		}
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(UIBase.AnchorType.Base);
		int width = rootAnchor.width;
		base.Widget.width = Mathf.Min(width - ItemInfoTooltip.Width - 30, _baseWidth);
		Vector3 position = rootAnchor.GetPosition(0f, 0.5f);
		position.x += 10f;
		base.Widget.SetPosition(position, 0f, 0.5f);
		_layoutComponent.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (!_isConfirmed)
		{
			if (_callbackItemFunction != null)
			{
				_callbackItemFunction(null);
			}
			if (_callbackListFunction != null)
			{
				_callbackListFunction(null);
			}
		}
		_items = null;
		_inventory = null;
		_selectableCount = 1;
		_selectableCountGetter = null;
		_itemAmountGetter = null;
		_confirmText = null;
		_autoFillText = null;
		_cancelText = null;
		_filterFunc = null;
		_callbackItemFunction = null;
		_callbackListFunction = null;
		_callbackItemSelectChangedFunction = null;
		_callbackItemsSelectChangedFunction = null;
		_onClickTitle = null;
		_isTitleOverride = false;
		_hasHelperText = false;
		PlayerController.MotionUpdater.RefreshMotion("Avatar_Ransack");
		_itemList.DeselectAllItems(sendEvent: false);
		if (_itemInfoTooltip != null)
		{
			_itemInfoTooltip.Hide();
			_itemInfoTooltip = null;
		}
	}

	public void AttachLoadingRingToHelperLabel()
	{
		LoadingRingWidget loadingRing = UIManager.Popup.LoadingRing;
		loadingRing.AttachToWidget(_helperLabel.gameObject, new Vector3(_helperLabel.printedSize.x + (float)loadingRing.GetComponent<UIWidget>().width / 2f, 0f));
	}

	public void DetachLoadingRingFromHelperLabel()
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(_helperLabel.gameObject);
	}
}

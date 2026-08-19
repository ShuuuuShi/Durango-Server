using System;
using System.Collections.Generic;
using ItemSystem;
using L10N;
using UnityEngine;

public class PopupItemSelector : TooltipBase
{
	[SerializeField]
	private ItemList _itemList;

	[SerializeField]
	private GameObject _background;

	[SerializeField]
	private DefaultSelectableButton _okBtn;

	[SerializeField]
	private DefaultSelectableButton _cancelBtn;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	private List<ItemData> _items;

	private int _selectibleCount;

	private string _title;

	private bool _displayTooltip;

	private ItemInfoPopup _itemInfoTooltip;

	private Func<ItemData, bool> _filterFunc;

	private Util.ItemDelegate _callbackItemFunction;

	private Util.ItemListDelegate _callbackListFunction;

	private Util.ItemDelegate _callbackItemSelectChangedFunction;

	private Util.ItemListDelegate _callbackItemsSelectChangedFunction;

	public ItemList ItemList => _itemList;

	protected override void OnAwake()
	{
		_okBtn.Clicked = OnConfirm;
		_cancelBtn.Clicked = OnCancel;
		_itemList.FixedIconSize = true;
		_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
	}

	private void OnConfirm()
	{
		if (!_okBtn.Disable)
		{
			if (_callbackItemFunction != null)
			{
				_callbackItemFunction(_itemList.LastClickedItemData);
			}
			if (_callbackListFunction != null)
			{
				_callbackListFunction(SelectedItems());
			}
			_callbackItemFunction = null;
			_callbackListFunction = null;
			_callbackItemSelectChangedFunction = null;
			Hide();
		}
	}

	private void OnCancel()
	{
		DoCancel();
		Hide();
	}

	private ItemData[] SelectedItems()
	{
		ItemData[] array = new ItemData[_itemList.SelectedItemList.Count];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = _itemList.SelectedItemList[i].Item;
		}
		return array;
	}

	private void DoCancel()
	{
		if (_callbackItemFunction != null)
		{
			_callbackItemFunction(null);
		}
		if (_callbackListFunction != null)
		{
			_callbackListFunction(null);
		}
		_callbackItemFunction = null;
		_callbackListFunction = null;
		_callbackItemSelectChangedFunction = null;
	}

	private void OnUpdateSelectItem()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		ItemData lastClickedItemData = _itemList.LastClickedItemData;
		if (_callbackItemSelectChangedFunction != null)
		{
			_callbackItemSelectChangedFunction(lastClickedItemData);
		}
		if (_callbackItemsSelectChangedFunction != null)
		{
			_callbackItemsSelectChangedFunction(SelectedItems());
		}
		if (lastClickedItemData == null)
		{
			if ((Object)(object)_itemInfoTooltip != (Object)null)
			{
				_itemInfoTooltip.Hide();
			}
		}
		else if (_displayTooltip)
		{
			ItemInfoPopup itemInfoPopup = UIManager.Popup.Tooltip<ItemInfoPopup>();
			itemInfoPopup.Sign = 1;
			itemInfoPopup.Set(lastClickedItemData);
			itemInfoPopup.Show(base.Widget, Vector2.up * (-100f + (float)base.Widget.height * base.Widget.pivotOffset.y), 5f);
			itemInfoPopup.HideIgnoreParent = ((Component)this).transform;
			itemInfoPopup.HideArrow();
			itemInfoPopup.TouchedMe = TouchedInfoTooltip;
			itemInfoPopup.AddOnFinished(OnHideItemInfoTooltip);
			_itemInfoTooltip = itemInfoPopup;
		}
	}

	private void TouchedInfoTooltip()
	{
		VisibleTimeReset();
	}

	private void OnHideItemInfoTooltip()
	{
		_itemInfoTooltip = null;
	}

	public void OkButtonEnable(bool enable)
	{
		_okBtn.Disable = !enable;
	}

	public void Set(Func<ItemData, bool> filterFunc = null, int selectableCount = 1, string confirmText = null, bool displayTooltip = true, Util.ItemDelegate callbackItem = null, Util.ItemListDelegate callbackList = null, Util.ItemDelegate callbackItemChanged = null, Util.ItemListDelegate callbackItemsChanged = null)
	{
		Set(null, filterFunc, selectableCount, confirmText, displayTooltip, callbackItem, callbackList, callbackItemChanged, callbackItemsChanged);
	}

	public void Set(List<ItemData> itemList, Func<ItemData, bool> filterFunc = null, int selectableCount = 1, string confirmText = null, bool displayTooltip = true, Util.ItemDelegate callbackItem = null, Util.ItemListDelegate callbackList = null, Util.ItemDelegate callbackItemChanged = null, Util.ItemListDelegate callbackItemsChanged = null)
	{
		if (itemList == null)
		{
			itemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		}
		_items = itemList;
		_selectibleCount = selectableCount;
		_displayTooltip = displayTooltip;
		if (string.IsNullOrEmpty(confirmText))
		{
			confirmText = T._("확인");
		}
		_okBtn.Text = confirmText;
		_okBtn.Disable = false;
		_filterFunc = filterFunc;
		_callbackItemFunction = callbackItem;
		_callbackListFunction = callbackList;
		_callbackItemSelectChangedFunction = callbackItemChanged;
		_callbackItemsSelectChangedFunction = callbackItemsChanged;
	}

	public void SetTitle(string title)
	{
		if (base.IsVisible)
		{
			_titleLabel.text = title;
			_title = title;
		}
		else
		{
			_title = title;
		}
	}

	protected override void OnShow()
	{
		base.Sign = -1;
		base.OnShow();
		KSingleton<PlayerController>.Instance().Motion("Avatar_Ransack");
	}

	protected override void FillData()
	{
		_itemList.SetItemList(_items, _filterFunc);
		_itemList.SelectableCount = _selectibleCount;
		_titleLabel.text = ((!string.IsNullOrEmpty(_title)) ? _title : T._("아이템 선택 창"));
	}

	protected override void UpdateLayout()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Vector2 printedSize = _cancelBtn.TextLabel.printedSize;
		Vector2 printedSize2 = _okBtn.TextLabel.printedSize;
		float num = Mathf.Max(printedSize.x, printedSize2.x);
		int num2 = (int)num + 40;
		_cancelBtn.Widget.width = num2;
		((Component)_cancelBtn.TextLabel).transform.localPosition = new Vector3((float)num2 * -0.5f - 4f, 4f);
		NGUITools.UpdateWidgetCollider(((Component)_cancelBtn).gameObject);
		UIUtility.UpdateAnchors(((Component)_cancelBtn).transform);
		_okBtn.Widget.width = num2;
		Vector3 localPosition = ((Component)_cancelBtn).transform.localPosition;
		localPosition.x -= (float)(num2 + 5);
		((Component)_okBtn).transform.localPosition = localPosition;
		((Component)_okBtn.TextLabel).transform.localPosition = new Vector3((float)num2 * -0.5f - 4f, 4f);
		NGUITools.UpdateWidgetCollider(((Component)_okBtn).gameObject);
		UIUtility.UpdateAnchors(((Component)_okBtn).transform);
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		DoCancel();
		KSingleton<PlayerController>.Instance().RefreshMotion("Avatar_Ransack");
		_title = null;
		_itemList.ClearSelectItem(sendEvent: false);
		if ((Object)(object)_itemInfoTooltip != (Object)null)
		{
			_itemInfoTooltip.Hide();
			_itemInfoTooltip = null;
		}
	}

	protected override void OnMoveWidget()
	{
		base.OnMoveWidget();
		if ((Object)(object)_itemInfoTooltip != (Object)null)
		{
			_itemInfoTooltip.Hide();
			_itemInfoTooltip = null;
		}
	}
}

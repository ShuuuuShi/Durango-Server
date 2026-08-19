using System;
using System.Collections.Generic;
using System.Text;
using ItemSystem;
using L10N;
using MarketData;
using Messages;
using UnityEngine;

public class SellItemWidget : MonoBehaviour
{
	[SerializeField]
	private ItemList _itemList;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private SortOptionContainer _sortOption;

	[SerializeField]
	private PriceInputWidget _priceInput;

	[SerializeField]
	private DefaultSelectableButton _registerButton;

	[SerializeField]
	private SimilarItemsWidget _similarItems;

	[SerializeField]
	private Selectable _similarItemsButton;

	private AnimationWidget _animWidget;

	private bool _isOpen;

	private List<ItemData> _filteredItemList;

	private bool _init;

	private AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public Market Market { get; set; }

	private void Init()
	{
		if (_init)
		{
			return;
		}
		_init = true;
		_itemList.SelectableCount = 1;
		_itemList.FixedIconSize = true;
		_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
		_sortOption.SortOptionSelected += OnSortItems;
		_priceInput.Init(1000, 100, 10, 1);
		_registerButton.Clicked = OnClickRegisterButton;
		Selectable similarItemsButton = _similarItemsButton;
		similarItemsButton.Clicked = (Action)Delegate.Combine(similarItemsButton.Clicked, (Action)delegate
		{
			ItemData lastClickedItemData = _itemList.LastClickedItemData;
			if (lastClickedItemData == null)
			{
				_similarItems.Hide();
			}
			else
			{
				_similarItems.Loading();
				GameSystem<MarketSystem>.Instance().GetSimilarProducts(lastClickedItemData, 3, OnReceiveSimilarItems);
			}
		});
	}

	private void Start()
	{
		Init();
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
		GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
		_itemList.ClearSelectItem(sendEvent: false);
		_priceInput.SetPrice(0);
	}

	public void Open(bool instant = false)
	{
		if (instant || !_isOpen)
		{
			_isOpen = true;
			((Component)this).gameObject.SetActive(true);
			if (instant)
			{
				AnimWidget.SetAlpha(1f, useTween: false);
			}
			else
			{
				AnimWidget.Delay = 0.2f;
				AnimWidget.Alpha = 1f;
			}
			OnUpdateInventory();
			OnUpdateSelectItem();
		}
	}

	public void Close(bool instant = false)
	{
		if (instant || _isOpen)
		{
			_isOpen = false;
			if (instant)
			{
				((Component)this).gameObject.SetActive(false);
				AnimWidget.SetAlpha(0f, useTween: false);
			}
			else
			{
				AnimWidget.Delay = 0f;
				AnimWidget.Alpha = 0f;
			}
		}
	}

	private void OnUpdateInventory()
	{
		if (_filteredItemList == null)
		{
			_filteredItemList = new List<ItemData>();
		}
		else
		{
			_filteredItemList.Clear();
		}
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		int i = 0;
		for (int count = playerItemList.Count; i < count; i++)
		{
			ItemData itemData = playerItemList[i];
			if (!itemData.IsEquipments)
			{
				_filteredItemList.Add(itemData);
			}
		}
		_itemList.SetItemList(_filteredItemList);
	}

	private void OnClickRegisterButton()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (_registerButton.Disable || _priceInput.GetPrice() <= 0)
		{
			return;
		}
		ItemData lastClickedItemData = _itemList.LastClickedItemData;
		if (lastClickedItemData == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(UIManager.ColorBBCode(UIManager.UIYellow));
		stringBuilder.Append(lastClickedItemData.Name);
		stringBuilder.Append("[-]");
		string comment = T._("{1}{0:-을} [557099]{2} [-]에 등록 하시겠습니까?", lastClickedItemData.Name, stringBuilder.ToString().Trim(), _priceInput.GetPrice().ToString("N0"));
		UIManager.MessageBox.Show(comment, delegate(bool ok)
		{
			if (ok)
			{
				RegisterCommodityItem();
			}
		});
	}

	private void RegisterCommodityItem()
	{
		if (GameSystem<MarketSystem>.Instance().RegisterCommodity(Market.Id, Market.Tile, _itemList.LastClickedItemData, _priceInput.GetPrice(), 86400f))
		{
			_itemList.ClearSelectItem();
		}
	}

	private void OnUpdateSelectItem()
	{
		ItemData lastClickedItemData = _itemList.LastClickedItemData;
		if (lastClickedItemData == null)
		{
			_itemInfo.Hide();
			((Component)_registerButton).gameObject.SetActive(false);
			_registerButton.Disable = true;
		}
		else
		{
			_itemInfo.Show(lastClickedItemData);
			((Component)_registerButton).gameObject.SetActive(true);
			_registerButton.Disable = false;
		}
	}

	private void OnSortItems(Util.SortOption option, bool isDescending)
	{
		InventorySystem inventorySystem = GameSystem<InventorySystem>.Instance();
		bool descending = isDescending;
		inventorySystem.SortItemList(option, null, descending);
		OnUpdateInventory();
	}

	private void OnReceiveSimilarItems(ItemData itemData, Commodity[] commodities)
	{
		if (object.Equals(itemData, _itemList.LastClickedItemData))
		{
			_similarItems.Show(commodities);
		}
	}
}

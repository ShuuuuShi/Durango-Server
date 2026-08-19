using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using NestedPrefab;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class SellItemWidget : AnimationWidget, IUIInitializable
{
	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private PriceInputWidget _priceInput;

	[SerializeField]
	private SelectableButton _registerButton;

	private readonly List<ItemData> _validItems = new List<ItemData>();

	private readonly List<ItemData> _invalidItems = new List<ItemData>();

	private ItemList _itemList;

	private bool _isOpen;

	void IUIInitializable.Init()
	{
		_itemList = _itemListLinker.Object.GetComponent<ItemList>();
		_itemList.SelectableCount = 1;
		_itemList.OnUpdateSelectItem = UpdateRegisterButton;
		_itemList.OnLongPress = delegate(ItemData item)
		{
			_itemList.SelectItem(item, sendEvent: true, scrollTo: true);
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
		};
		_priceInput.Init();
		_priceInput.PriceChanged += UpdateRegisterButton;
		_registerButton.Clicked = OnClickRegisterButton;
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdateInventory;
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdateInventory;
		_priceInput.SetPrice(0L);
	}

	public void Open(bool instant = false)
	{
		if (instant || !_isOpen)
		{
			_isOpen = true;
			base.gameObject.SetActive(value: true);
			if (instant)
			{
				SetAlpha(1f, useTween: false);
			}
			else
			{
				base.Delay = 0.2f;
				base.Alpha = 1f;
			}
			OnUpdateInventory();
			UpdateRegisterButton();
		}
	}

	public void Close(bool instant = false)
	{
		if (instant || _isOpen)
		{
			_isOpen = false;
			if (instant)
			{
				base.gameObject.SetActive(value: false);
				SetAlpha(0f, useTween: false);
			}
			else
			{
				base.Delay = 0f;
				base.Alpha = 0f;
			}
		}
	}

	private void OnUpdateInventory()
	{
		_validItems.Clear();
		_invalidItems.Clear();
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		for (int i = 0; i < playerItemList.Count; i++)
		{
			if (IsTradable(playerItemList[i]))
			{
				_validItems.Add(playerItemList[i]);
			}
			else
			{
				_invalidItems.Add(playerItemList[i]);
			}
		}
		_itemList.SetItemList(new ItemList.SetStruct[2]
		{
			new ItemList.SetStruct
			{
				List = _validItems
			},
			new ItemList.SetStruct
			{
				List = _invalidItems,
				OnInit = delegate(ItemIconWidget icon)
				{
					icon.IconMode = ItemIconWidget.Mode.Disabled;
				}
			}
		});
	}

	private bool IsTradable(ItemData item)
	{
		if (!item.Tradable)
		{
			return false;
		}
		if (item.IsEquipments)
		{
			return false;
		}
		if (item.Durability.Ratio() < 0.9f)
		{
			return false;
		}
		return true;
	}

	private void OnClickRegisterButton()
	{
		if (_priceInput.GetPrice() <= 0)
		{
			return;
		}
		List<ItemData> items = _itemList.SelectedList;
		if (KUtility.GetSize(items) == 0)
		{
			return;
		}
		ItemData itemData = items[0];
		long price = _priceInput.GetPrice();
		string text = Inventory.CurrencyFormat(price, Currency.TStone);
		string comment;
		if (items.Count == 1)
		{
			comment = itemData.SafeLevel switch
			{
				SafeLevel.Locked => T._("<em>잠금</em> 설정된 <em>{0}</em>{0:-을} <em>{1:N0}</em> 에 등록하시겠습니까?", itemData.Name, text), 
				SafeLevel.Protected => T._("<em>임무</em> 수행에 필요한 <em>{0}</em>{0:-을} <em>{1:N0}</em> 에 등록하시겠습니까?", itemData.Name, text), 
				_ => T._("<em>{0}</em>{0:-을} <em>{1:N0}</em> 에 등록하시겠습니까?", itemData.Name, text), 
			};
		}
		else
		{
			ItemData itemData2 = items.MaxBy((ItemData x) => x.SafeLevel);
			comment = (itemData2?.SafeLevel ?? SafeLevel.None) switch
			{
				SafeLevel.Locked => T._("<em>잠금</em> 설정된 <em>{0}</em> 외 {1}개 물품을 각각 <em>{2:N0}</em>에 등록하시겠습니까?", itemData2.Name, items.Count - 1, text), 
				SafeLevel.Protected => T._("<em>임무</em> 수행에 필요한 <em>{0}</em> 외 {1}개 물품을 각각 <em>{2:N0}</em>에 등록하시겠습니까?", itemData2.Name, items.Count - 1, text), 
				_ => T._("<em>{0}</em> 외 {1}개 물품을 각각 <em>{2:N0}</em>에 등록하시겠습니까?", itemData.Name, items.Count - 1, text), 
			};
		}
		long listingFee = Singleton<Constants>.Instance.Market.GetListingFee(price);
		string subText = T._("<alert_icon/> 등록 수수료 {0} 은 선불이며 환불되지 않습니다.", Inventory.CurrencyFormat(listingFee, Currency.TStone));
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.AddKeyValueInfo(value: Inventory.CurrencyFormat(price - Singleton<Constants>.Instance.Market.GetSalesFee(price), Currency.TStone), key: T._("(판매 수수료 제외) 판매 수익"));
		messageBox.ShowPayConfirm(listingFee, Currency.TStone, comment, subText, delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<MarketSystem>.Instance().RegisterCommodity(items, _priceInput.GetPrice(), 86400f);
			}
		});
	}

	private void UpdateRegisterButton()
	{
		ItemData lastClickedItem = _itemList.LastClickedItem;
		if (lastClickedItem != null && !IsTradable(lastClickedItem))
		{
			if (!lastClickedItem.Tradable)
			{
				UIManager.SystemMsg("MarketWarning", T._("거래할 수 없는 아이템입니다."));
				return;
			}
			if (lastClickedItem.IsEquipments)
			{
				UIManager.SystemMsg("MarketWarning", T._("착용중인 아이템은 등록할 수 없습니다."));
				return;
			}
			if (lastClickedItem.Durability.Ratio() < 0.9f)
			{
				UIManager.SystemMsg("MarketWarning", T._("내구도가 {0:P0} 미만인 아이템은 등록할 수 없습니다", 0.9f));
				return;
			}
		}
		ItemData lastSelectedItem = _itemList.LastSelectedItem;
		if (lastSelectedItem == null)
		{
			_itemInfo.Hide();
			_registerButton.gameObject.SetActive(value: false);
			_registerButton.Disabled = true;
			_priceInput.InsertAlarm(on: false);
		}
		else
		{
			_itemInfo.Show(lastSelectedItem);
			_registerButton.gameObject.SetActive(value: true);
			long price = _priceInput.GetPrice();
			_registerButton.Disabled = price <= 0;
			_priceInput.InsertAlarm(price <= 0);
		}
		int count = _itemList.SelectedList.Count;
		_registerButton.Text = ((count <= 1) ? T._("등록") : T._("{0}개 등록", count));
	}
}

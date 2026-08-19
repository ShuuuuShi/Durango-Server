using System;
using System.Collections.Generic;
using Building_;
using ItemSystem;
using L10N;
using UnityEngine;

public class InventoryContainer : MonoBehaviour
{
	[SerializeField]
	private KScrollView _tabList;

	[SerializeField]
	private WarehouseTabSelector _warehouseTabSelector;

	[SerializeField]
	private WarehouseTabConfig _warehouseTabConfig;

	[SerializeField]
	private Selectable _warehouseTabConfigBtn;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private GameObject _buttonContainer;

	[SerializeField]
	private SortOptionContainer _sortOption;

	[SerializeField]
	private GameObject _multipleSelectBtn;

	[SerializeField]
	private InventoryActionButtons _actionButtons;

	[SerializeField]
	private ItemList _itemList;

	[SerializeField]
	private GameObject _sortOptionContainer;

	[SerializeField]
	private UISprite _inventorySizeIcon;

	[SerializeField]
	private UILabel _textInventorySize;

	[SerializeField]
	private AudioClipType _itemDestroyedAuido;

	private int _selectedTab;

	private ulong _selectedItem;

	private Inventory _player;

	private Inventory _other;

	private Inventory.InventoryMode _inventoryMode = Inventory.InventoryMode.Invaild;

	public ItemList ItemList => _itemList;

	public InventoryActionButtons Buttons => _actionButtons;

	public bool ItemsRepositionFlag { get; private set; }

	public event Action Closed;

	private void Awake()
	{
		SoundManager.Cache(_itemDestroyedAuido);
		_buttonContainer.SetActive(false);
		((Component)_itemList).gameObject.SetActive(true);
		_actionButtons.UseTypeToString = UseTypeToString;
		UIEventListener.Get(_multipleSelectBtn).onClick = delegate
		{
			_itemList.ClearSelectItem();
			MultipleSelectMode(_itemList.SelectableCount == 1);
		};
		_actionButtons.OnUse += OnUseItem;
		_actionButtons.OnRemove += AskDropSelectItem;
		_actionButtons.OnLock += OnLockItem;
		_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
		_itemList.OnChangeItemList = CheckValidItemInfo;
		_itemList.OnLongPress = delegate(ItemIcon2 icon)
		{
			if (_itemList.SelectableCount <= 1)
			{
				_itemList.SelectItem(icon.Item);
				MultipleSelectMode(enable: true);
			}
		};
		_itemList.SelectableCount = 1;
		_itemList.EquipmentsSelectable = true;
		_sortOption.SortOptionSelected += OnSortItemList;
		_tabList.Nodes.Init(delegate(GameObject o)
		{
			UIEventListener uIEventListener = UIEventListener.Get(o);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTabSelect));
		});
		Selectable warehouseTabConfigBtn = _warehouseTabConfigBtn;
		warehouseTabConfigBtn.Clicked = (Action)Delegate.Combine(warehouseTabConfigBtn.Clicked, (Action)delegate
		{
			_warehouseTabConfig.Show();
		});
	}

	private void OnEnable()
	{
		ItemsRepositionFlag = false;
		_selectedTab = -1;
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += Refresh;
		GameSystem<InventorySystem>.Instance().TrakingInventoryUpdated += Refresh;
		SetInventoryMode(_inventoryMode);
		GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		Refresh();
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= Refresh;
		GameSystem<InventorySystem>.Instance().TrakingInventoryUpdated -= Refresh;
		_inventoryMode = Inventory.InventoryMode.Normal;
		_itemList.ClearSelectItem();
		MultipleSelectMode(enable: false);
	}

	private Inventory GetCurrentInventory()
	{
		return GetInventory(_selectedTab);
	}

	private Inventory GetInventory(int index)
	{
		if (index == 0)
		{
			return _player;
		}
		if (index > 0)
		{
			return _other;
		}
		return null;
	}

	private List<ItemData> GetCurrentItemList()
	{
		return GetCurrentInventory()?.Items;
	}

	private string GetCurrentCategory()
	{
		if (_other == null || _other.Type != Inventory.InventoryType.Warehouse)
		{
			return null;
		}
		int size = KUtility.GetSize(_other.Categories);
		int num = _selectedTab - 1;
		if (num < 0 || num >= size)
		{
			return null;
		}
		return _other.Categories[num].Key;
	}

	private void MultipleSelectMode(bool enable)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (enable)
		{
			_itemList.SelectableCount = -1;
			_multipleSelectBtn.GetComponent<UISprite>().color = UIManager.UIYellow;
		}
		else if (_inventoryMode == Inventory.InventoryMode.Normal)
		{
			_itemList.SelectableCount = 1;
			_multipleSelectBtn.GetComponent<UISprite>().color = UIManager.UIWhite;
		}
	}

	private void Close()
	{
		if (this.Closed != null)
		{
			this.Closed();
		}
	}

	private void OnUpdateSelectItem()
	{
		ShowItemInfo((_itemList.SelectedItemList.Count <= 0) ? null : _itemList.SelectedItemList[_itemList.SelectedItemList.Count - 1].Item);
	}

	private void Refresh()
	{
		_player = GameSystem<InventorySystem>.Instance().PlayerInventory;
		_other = GameSystem<InventorySystem>.Instance().TrakingInventory;
		if (_other.OwnerId == 0L)
		{
			_other = null;
		}
		if (_selectedTab == -1)
		{
			if (_other != null && _other.Type == Inventory.InventoryType.Artifact)
			{
				_selectedTab = 1;
			}
			else
			{
				_selectedTab = 0;
			}
		}
		UpdateInventoryInfo();
		UpdateTab();
		UpdateItemList();
		if (_other != null && _other.Type == Inventory.InventoryType.Warehouse && _other.SelectedCategory == null && _selectedTab > 0)
		{
			GameSystem<InventorySystem>.Instance().GetWarehouseCategory(_selectedTab - 1);
		}
	}

	private void UpdateInventoryInfo()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Inventory currentInventory = GetCurrentInventory();
		int num = currentInventory?.CurrentSize() ?? 0;
		float num2 = currentInventory?.Capacity ?? 1f;
		string arg = NGUIText.EncodeColor(num.ToString("0"), (!((float)num > num2)) ? Color.white : Color.red);
		_inventorySizeIcon.fillAmount = ((num <= 0) ? 0f : (0.09f + 0.85f * ((float)num / num2)));
		_textInventorySize.text = $"{arg} [3f3f3f]/[-] {num2:0}";
		UIUtility.UpdateAnchors(((Component)_textInventorySize).transform.parent);
	}

	private void UpdateItemList()
	{
		_itemList.SetItemList(GetCurrentItemList());
		ItemIcon2 itemIcon = _itemList.Find(_selectedItem);
		ShowItemInfo((!((Object)(object)itemIcon == (Object)null)) ? itemIcon.Item : null);
	}

	private void UpdateTab()
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		if (_other == null)
		{
			((Component)_tabList).gameObject.SetActive(false);
			return;
		}
		((Component)_tabList).gameObject.SetActive(true);
		ListObjectPool nodes = _tabList.Nodes;
		nodes.Clear();
		KeyValueLabel keyValueLabel = ((ListObjectPoolBase<GameObject>)nodes).Add<KeyValueLabel>();
		keyValueLabel.Set(T._("가방"), null);
		if (_other.Type == Inventory.InventoryType.Warehouse)
		{
			int i = 0;
			for (int size = KUtility.GetSize(_other.Categories); i < size; i++)
			{
				KeyValueLabel keyValueLabel2 = ((ListObjectPoolBase<GameObject>)nodes).Add<KeyValueLabel>();
				keyValueLabel2.Set(_other.Categories[i].Key, null);
			}
			((Component)_warehouseTabConfigBtn).gameObject.SetActive(true);
		}
		else
		{
			KeyValueLabel keyValueLabel3 = ((ListObjectPoolBase<GameObject>)nodes).Add<KeyValueLabel>();
			keyValueLabel3.Set(TargetInventoryName(_other, T._("알수없음")), null);
			((Component)_warehouseTabConfigBtn).gameObject.SetActive(false);
		}
		float num = 0f;
		for (int j = 0; j < nodes.Count; j++)
		{
			KeyValueLabel component = nodes[j].GetComponent<KeyValueLabel>();
			num = Mathf.Max(component.GetPredictSize().x, num);
		}
		int width = (int)num + 50;
		for (int k = 0; k < nodes.Count; k++)
		{
			KeyValueLabel component2 = nodes[k].GetComponent<KeyValueLabel>();
			Inventory inventory = GetInventory(k);
			if (inventory != null)
			{
				if (inventory.Type == Inventory.InventoryType.Warehouse)
				{
					component2.SetValue($"{inventory.Categories[k - 1].Value:0} / {200:0}");
				}
				else if (inventory.Capacity > 0f)
				{
					component2.SetValue($"{inventory.CurrentSize():0} / {inventory.Capacity:0}");
				}
			}
			component2.UpdateLayout(width);
			Selectable component3 = nodes[k].GetComponent<Selectable>();
			component3.Select = k == _selectedTab;
		}
		_tabList.Reposition();
	}

	private void OnTabSelect(GameObject obj)
	{
		int num = _tabList.Nodes.IndexOf(obj);
		if (_selectedTab != num)
		{
			_selectedTab = num;
			if (_selectedTab > 0 && _other != null && _other.Type == Inventory.InventoryType.Warehouse)
			{
				GameSystem<InventorySystem>.Instance().GetWarehouseCategory(_selectedTab - 1);
			}
			_itemList.ClearSelectItem();
			Refresh();
			_itemList.ResetPosition();
		}
	}

	private string TargetInventoryName(Inventory inventory, string unknownText = null)
	{
		if (inventory == null)
		{
			return unknownText;
		}
		switch (inventory.Type)
		{
		case Inventory.InventoryType.Rein:
		{
			ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(inventory.OwnerId);
			if (itemData != null && itemData.Reins != null)
			{
				return itemData.Reins.PetName;
			}
			break;
		}
		case Inventory.InventoryType.Artifact:
		{
			Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(inventory.OwnerId);
			if ((Object)(object)artifact != (Object)null)
			{
				return artifact.LocalizedName;
			}
			break;
		}
		}
		return unknownText;
	}

	private void ShowItemInfo(ItemData item)
	{
		_selectedItem = item?.Id ?? 0;
		if (item == null)
		{
			_itemInfo.Hide();
			_buttonContainer.SetActive(false);
			_actionButtons.UseType = UseType.None;
		}
		else
		{
			_itemInfo.Show(item);
			_buttonContainer.SetActive(true);
			_actionButtons.UpdateUseButtonAction(GetCurrentInventory(), _inventoryMode, _itemList.SelectedItemList);
		}
	}

	private void CheckValidItemInfo()
	{
		if ((Object)(object)_itemList.Find(_selectedItem) == (Object)null)
		{
			ShowItemInfo(null);
		}
	}

	private string UseTypeToString(UseType type)
	{
		switch (type)
		{
		case UseType.PutIn:
		{
			if (_other != null && _other.Type == Inventory.InventoryType.Warehouse)
			{
				return T._("옮기기");
			}
			string text = TargetInventoryName(_other);
			return (!string.IsNullOrEmpty(text)) ? T._("{0:으로} 옮기기", text) : type.GetName();
		}
		case UseType.TakeOut:
			if (_other != null && _other.Type == Inventory.InventoryType.Warehouse)
			{
				return T._("옮기기");
			}
			break;
		case UseType.ToggleSpawn:
			if (_itemList.SelectedItemList.Count == 1)
			{
				ItemData item = _itemList.SelectedItemList[0].Item;
				if (item.Reins != null && KSingleton<AnimalManager>.HasInstance())
				{
					return (!Object.op_Implicit((Object)(object)KSingleton<AnimalManager>.Instance().GetAnimal(item.Id))) ? T._("소환") : T._("해제");
				}
			}
			break;
		}
		return type.GetName();
	}

	public void SetInventoryMode(Inventory.InventoryMode mode)
	{
		_inventoryMode = mode;
		if (NGUITools.GetActive((Behaviour)(object)this))
		{
			switch (_inventoryMode)
			{
			case Inventory.InventoryMode.Normal:
				((Component)_tabList).gameObject.SetActive(false);
				_actionButtons.SetBottomButtonLayout(2f, 1f, 1f);
				MultipleSelectMode(enable: false);
				break;
			case Inventory.InventoryMode.Dead:
				((Component)_tabList).gameObject.SetActive(false);
				_actionButtons.SetBottomButtonLayout(2f, 1f, 1f);
				MultipleSelectMode(enable: true);
				break;
			case Inventory.InventoryMode.Exchange:
				((Component)_tabList).gameObject.SetActive(true);
				_actionButtons.SetBottomButtonLayout(1f, 0f, 0f);
				MultipleSelectMode(enable: true);
				break;
			}
		}
	}

	private void CopySelectedItem()
	{
		ItemIcon2 itemIcon = _itemList.Find(_selectedItem);
		ItemData itemData = ((!((Object)(object)itemIcon == (Object)null)) ? itemIcon.Item : null);
		if (itemData != null)
		{
			int i = 0;
			for (int count = _itemList.SelectedItemList.Count; i < count; i++)
			{
				string cheat = $"copy item {_itemList.SelectedItemList[i].Item.Id} 1";
				KSingleton<Commands>.Instance().Cheat(cheat);
			}
		}
	}

	private void OnUseItem()
	{
		if (_itemList.SelectedItemList.Count == 0)
		{
			return;
		}
		List<ItemData> list = new List<ItemData>();
		int i = 0;
		for (int count = _itemList.SelectedItemList.Count; i < count; i++)
		{
			list.Add(_itemList.SelectedItemList[i].Item);
		}
		UseType useType = _actionButtons.UseType;
		switch (useType)
		{
		case UseType.TakeOut:
			_itemList.ClearSelectItem();
			break;
		case UseType.PutIn:
			if (_itemList.Count - list.Count <= 0)
			{
				Close();
			}
			_itemList.ClearSelectItem();
			break;
		case UseType.Eat:
		case UseType.Drink:
		case UseType.ToggleSpawn:
		case UseType.Place:
		case UseType.Resurrection_Rewards:
		case UseType.PackArtifact:
		case UseType.UnpackArtifact:
			Close();
			break;
		}
		ItemAction(useType, list);
	}

	private void ItemAction(UseType type, IList<ItemData> items)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		switch (type)
		{
		case UseType.TakeOut:
			switch (_other.Type)
			{
			case Inventory.InventoryType.Artifact:
				InventorySystem.TakeOutItems(_other.OwnerId, _other.OwnerPosition, Util.ItemsToIds(items));
				break;
			case Inventory.InventoryType.Rein:
				InventorySystem.TakeOutItemsFromPet(_other.OwnerId, Util.ItemsToIds(items));
				break;
			case Inventory.InventoryType.Warehouse:
			{
				int num2 = 0;
				for (int j = 0; j < items.Count; j++)
				{
					num2 += items[j].Size;
				}
				string current = GetCurrentCategory();
				_warehouseTabSelector.Show(_other, true, delegate(string category)
				{
					if (category == null)
					{
						InventorySystem.TakeOutItemsFromWarehouse(_other.OwnerId, _other.OwnerPosition, current, Util.ItemsToIds(items));
					}
					else
					{
						InventorySystem.MoveToItemsFromWarehouse(_other.OwnerId, _other.OwnerPosition, current, category, Util.ItemsToIds(items));
					}
				}, num2, current);
				break;
			}
			}
			break;
		case UseType.PutIn:
			switch (_other.Type)
			{
			case Inventory.InventoryType.Artifact:
				InventorySystem.PutInItems(_other.OwnerId, _other.OwnerPosition, Util.ItemsToIds(items));
				break;
			case Inventory.InventoryType.Rein:
				InventorySystem.PutInItemsIntoPet(_other.OwnerId, Util.ItemsToIds(items));
				break;
			case Inventory.InventoryType.Warehouse:
			{
				int num = 0;
				for (int i = 0; i < items.Count; i++)
				{
					num += items[i].Size;
				}
				_warehouseTabSelector.Show(_other, false, delegate(string category)
				{
					InventorySystem.PutInItemsIntoWarehouse(_other.OwnerId, _other.OwnerPosition, category, Util.ItemsToIds(items));
				}, num);
				break;
			}
			}
			break;
		case UseType.Eat:
		case UseType.Drink:
		case UseType.ToggleSpawn:
			GameSystem<InventorySystem>.Instance().UseItem(items[0]);
			break;
		case UseType.Equip:
			GameSystem<EquipSystem>.Instance().EquipItem(items[0]);
			break;
		case UseType.UnEquip:
			GameSystem<EquipSystem>.Instance().EquipItem(items[0]);
			break;
		case UseType.Resurrection_Rewards:
			GameSystem<InventorySystem>.Instance().SetResurrectionReward(items);
			break;
		case UseType.Place:
		{
			ItemData item = items[0];
			Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(item.Capsule.BlueprintId);
			UIManager.FindScript<BuildGridGroup>().Open(blueprint, delegate
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				BuildManager buildManager = KSingleton<BuildManager>.Instance();
				GameSystem<BuildSystem>.Instance().PlaceCapsulatedArtifact(item.Id, blueprint.Icon, buildManager.WorldTilePos, buildManager.Rotated, buildManager.Center);
			});
			break;
		}
		case UseType.PackArtifact:
		case UseType.UnpackArtifact:
			UIManager.Open<PackArtifactGroup>();
			break;
		case UseType.CheatCopy:
			CopySelectedItem();
			break;
		case UseType.Water:
		case UseType.Repair:
			break;
		}
	}

	private void OnLockItem()
	{
		int size = KUtility.GetSize(_itemList.SelectedItemList);
		switch (size)
		{
		case 0:
			return;
		case 1:
			GameSystem<InventorySystem>.Instance().LikeItem(_itemList.LastClickedItemData);
			return;
		}
		ItemData[] array = new ItemData[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = _itemList.SelectedItemList[i].Item;
		}
		GameSystem<InventorySystem>.Instance().LikeItem(array);
	}

	private void AskDropSelectItem()
	{
		if (_itemList.SelectedItemList == null || _itemList.SelectedItemList.Count == 0)
		{
			return;
		}
		UIManager.MessageBox.Show(T._("아이템을 버리시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				DropSelectedItem();
			}
		});
	}

	private void DropSelectedItem()
	{
		if (_itemList.SelectedItemList.Count > 0)
		{
			RequestDropCheckItems();
		}
		else if (!((Object)(object)_itemList.LastClickedItem == (Object)null))
		{
			RequestDropItem(_itemList.LastClickedItemData);
		}
	}

	private void RequestDropCheckItems()
	{
		int count = _itemList.SelectedItemList.Count;
		ItemData[] array = new ItemData[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = _itemList.SelectedItemList[i].Item;
		}
		SoundManager.Play((string)_itemDestroyedAuido, loop: false, default(SoundManager.PitchRange));
		GameSystem<InventorySystem>.Instance().DropItems(array);
	}

	private void RequestDropItem(ItemData item)
	{
		if (item != null)
		{
			SoundManager.Play((string)_itemDestroyedAuido, loop: false, default(SoundManager.PitchRange));
			GameSystem<InventorySystem>.Instance().DropItems(item);
		}
	}

	private void OnSortItemList(Util.SortOption option, bool descending)
	{
		List<ItemData> currentItemList = GetCurrentItemList();
		if (currentItemList != null)
		{
			Util.SortItems(currentItemList, option, descending);
			_itemList.SetItemList(currentItemList);
			ItemsRepositionFlag = true;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Building;
using Durango.Logic;
using Durango.Logic.Clan;
using Durango.Logic.Estate;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.Network;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.InGame;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Estate;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class InventoryContainerBase : MonoBehaviour, IUIInitializable
{
	private class MarketMainCategoryComparer : IEqualityComparer<Category.Main>
	{
		public bool Equals(Category.Main tup1, Category.Main tup2)
		{
			if (tup1 == null && tup2 == null)
			{
				return true;
			}
			if (tup1 == null || tup2 == null)
			{
				return false;
			}
			if (tup1.Id == tup2.Id && tup1.Id == tup2.Id)
			{
				return true;
			}
			return false;
		}

		public int GetHashCode(Category.Main t)
		{
			return t.Id.GetHashCode();
		}
	}

	public class TagPriorityComparer : Comparer<ItemData>
	{
		private readonly string[] _high;

		private readonly string[] _low;

		private readonly int _orderSign;

		private readonly Comparison<ItemData> _defaultComparer;

		public TagPriorityComparer([NotNull] string[] high, string[] low, Comparison<ItemData> comparer, bool isReversedDefaultSortOption)
		{
			_high = high;
			_low = low;
			_defaultComparer = comparer;
			_orderSign = ((!isReversedDefaultSortOption) ? 1 : (-1));
		}

		private int GetGrade(ItemData item)
		{
			if (item == null)
			{
				return 10;
			}
			int i = 0;
			for (int size = KUtility.GetSize(_high); i < size; i++)
			{
				if (item.HasTag(_high[i]))
				{
					return 0;
				}
			}
			int j = 0;
			for (int size2 = KUtility.GetSize(_low); j < size2; j++)
			{
				if (item.HasTag(_low[j]))
				{
					return 2;
				}
			}
			return 1;
		}

		public override int Compare(ItemData i1, ItemData i2)
		{
			int grade = GetGrade(i1);
			int grade2 = GetGrade(i2);
			if (grade == grade2)
			{
				return _orderSign * _defaultComparer(i1, i2);
			}
			return grade - grade2;
		}
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private GameObject[] _onlySingleSelectModeObject;

	[SerializeField]
	protected bool _pinnedTabList;

	[SerializeField]
	protected KScrollView _tabList;

	[SerializeField]
	private WarehouseTabSelector _warehouseTabSelector;

	[SerializeField]
	private WarehouseTabConfig _warehouseTabConfig;

	[SerializeField]
	protected Selectable _warehouseTabAddBtn;

	[SerializeField]
	protected Selectable _warehouseTabConfigBtn;

	[SerializeField]
	private KScrollView _categoryTabList;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private GameObject _buttonContainer;

	[SerializeField]
	private SelectableButton _multiselectButton;

	[SerializeField]
	private InventoryActionButtons _actionButtons;

	[SerializeField]
	private InventoryMenuBarBase _menuBar;

	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[CanBeNull]
	[SerializeField]
	private UISprite _inventorySizeIcon;

	[CanBeNull]
	[SerializeField]
	private UILabel _textInventorySize;

	[SerializeField]
	private SoundEventType _rearrangeSound;

	[SerializeField]
	private RectLayout _layout;

	private int _prevSelectedTab;

	protected int _selectedTab;

	private readonly string[] _categoryIdList = new string[11]
	{
		"all", "food/medicine", "plant_collectible", "material", "animal_collectible", "building/furniture", "mineral", "weapon/tool", "seed", "clothing",
		"taming"
	};

	private readonly string[] _categoryIdListForWarprush = new string[4] { "all", "food/medicine", "weapon/tool", "clothing" };

	private int _selectedCategoryTabIndex;

	protected Durango.Logic.Item.Inventory _player;

	protected Durango.Logic.Item.Inventory _other;

	private Durango.Logic.Item.Inventory.InventoryMode _inventoryMode;

	private readonly HashSet<Category.Main> _categoryFilters = new HashSet<Category.Main>(new MarketMainCategoryComparer());

	private readonly HashSet<string> _tagFilters = new HashSet<string>();

	private readonly List<UseType> _usableList = new List<UseType>();

	protected ItemList _itemList;

	private bool _isDirty;

	private bool IsFilterApplied => _tagFilters.Count > 0;

	public ItemList ItemList => _itemList;

	public InventoryActionButtons Buttons => _actionButtons;

	protected bool IsMultiselect => _itemList.IsMultiSelectMode;

	private bool IsEveryItemSelected => _itemList.SelectedList.Count == _itemList.Count;

	void IUIInitializable.Init()
	{
		_itemList = _itemListLinker.Object.GetComponent<ItemList>();
		_buttonContainer.SetActive(value: false);
		_actionButtons.UseTypeToString = UseTypeToString;
		_multiselectButton.Clicked = delegate
		{
			MultiselectMode(!IsMultiselect);
		};
		_actionButtons.OnUse += OnUseItem;
		_menuBar.OnSort += OnSortItemList;
		_menuBar.OnRemove += AskDropSelectItem;
		_menuBar.OnLock += OnLockItem;
		_menuBar.OnSelectAll += OnSelectAllItem;
		_menuBar.OnFilter += OnFilterItem;
		_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
		_itemList.OnChangeItemList = CheckValidItemInfo;
		_itemList.OnLongPress = delegate(ItemData item)
		{
			if (IsMultiselect)
			{
				_itemList.ToggleSimillarItems(item.PrototypeId);
			}
			else
			{
				MultiselectMode(enable: true);
				_itemList.SelectItem(item, sendEvent: true, scrollTo: true);
				UISound.PlayClick(UISound.ClickType.ButtonDefault);
			}
		};
		_itemList.SelectableCount = 1;
		_itemList.EquipmentsSelectable = true;
		_tabList.Nodes.Init(delegate(GameObject o)
		{
			UIEventListener uIEventListener = UIEventListener.Get(o);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTabSelect));
		});
		_warehouseTabConfigBtn.gameObject.SetActive(value: false);
		_warehouseTabAddBtn.gameObject.SetActive(value: false);
		Selectable warehouseTabAddBtn = _warehouseTabAddBtn;
		warehouseTabAddBtn.Clicked = (Action)Delegate.Combine(warehouseTabAddBtn.Clicked, (Action)delegate
		{
			UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string text)
			{
				GameSystem<InventorySystem>.Instance().AddWarehouseCategory(text);
			}, T._("새로 추가할 탭을 적어주세요"));
		});
		Selectable warehouseTabConfigBtn = _warehouseTabConfigBtn;
		warehouseTabConfigBtn.Clicked = (Action)Delegate.Combine(warehouseTabConfigBtn.Clicked, (Action)delegate
		{
			_warehouseTabConfig.Show();
		});
		_categoryTabList.Nodes.BeginLoad();
		string[] list = ((!GameManager.Region.IsPvpIsland()) ? _categoryIdList : _categoryIdListForWarprush);
		string[] array = list;
		foreach (string text2 in array)
		{
			SelectableWidget component = _categoryTabList.Nodes.GetNext().GetComponent<SelectableWidget>();
			component.gameObject.FindComponent<UISprite>("Icon").spriteName = string.Format("bag_category_{0}", text2.Replace("/", "_"));
			string id = text2;
			component.Clicked = (Action)Delegate.Combine(component.Clicked, (Action)delegate
			{
				int num = list.IndexOf(id);
				if (num >= 0)
				{
					_categoryTabList.Nodes.Get<SelectableWidget>(_selectedCategoryTabIndex).Selected = false;
					_categoryTabList.Nodes.Get<SelectableWidget>(num).Selected = true;
					_selectedCategoryTabIndex = num;
					List<Category.Main> list2 = new List<Category.Main>();
					if (id != "all")
					{
						list2.Add(new Category.Main(id));
						if (id == "clothing")
						{
							list2.Add(new Category.Main("accessory"));
						}
					}
					ApplyCategoryFilter(list2);
				}
			});
		}
		_categoryTabList.Nodes.EndLoad();
		_selectedCategoryTabIndex = 0;
		_categoryTabList.Nodes.Get<SelectableWidget>(_selectedCategoryTabIndex).Selected = true;
	}

	private void OnEnable()
	{
		_selectedTab = -1;
		_prevSelectedTab = -1;
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += Refresh;
		GameSystem<InventorySystem>.Instance().TrackingInventoryUpdated += Refresh;
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated += EquipmentsUpdated;
		SetInventoryMode(_inventoryMode);
		LateRefresh();
		_categoryTabList.Nodes.Get<SelectableWidget>(0).Clicked();
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= Refresh;
		GameSystem<InventorySystem>.Instance().TrackingInventoryUpdated -= Refresh;
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated -= EquipmentsUpdated;
		_inventoryMode = Durango.Logic.Item.Inventory.InventoryMode.Normal;
		_itemList.DeselectAllItems(sendEvent: true);
		MultiselectMode(enable: false);
		_categoryTabList.Nodes.Get<SelectableWidget>(0).Clicked();
		_tagFilters.Clear();
	}

	private void Update()
	{
		if (_isDirty)
		{
			LateRefresh();
		}
	}

	public bool OnClose()
	{
		if (_inventoryMode == Durango.Logic.Item.Inventory.InventoryMode.Normal && IsMultiselect)
		{
			MultiselectMode(enable: false);
			return false;
		}
		if (_warehouseTabSelector.IsVisible)
		{
			_warehouseTabSelector.Hide();
			return false;
		}
		if (_warehouseTabConfig.IsVisible)
		{
			_warehouseTabConfig.Hide();
			return false;
		}
		return true;
	}

	[CanBeNull]
	private Durango.Logic.Item.Inventory GetCurrentInventory()
	{
		return GetInventory(_selectedTab);
	}

	[CanBeNull]
	protected Durango.Logic.Item.Inventory GetInventory(int index)
	{
		if (index == 0)
		{
			return _player;
		}
		return (index <= 0) ? null : _other;
	}

	private List<ItemData> GetCurrentItemList()
	{
		return GetCurrentInventory()?.Items;
	}

	private Action<ItemIconWidget> GetCurrentInitFunc()
	{
		if (_selectedTab == 0 && _other != null && (KUtility.GetSize(_other.StorableTags) > 0 || KUtility.GetSize(_other.UnstorableTags) > 0))
		{
			return delegate(ItemIconWidget icon)
			{
				bool flag = KUtility.GetSize(_other.StorableTags) == 0;
				int i = 0;
				for (int size = KUtility.GetSize(_other.StorableTags); i < size; i++)
				{
					if (icon.Item.HasTag(_other.StorableTags[i]))
					{
						flag = true;
						break;
					}
				}
				int j = 0;
				for (int size2 = KUtility.GetSize(_other.UnstorableTags); j < size2; j++)
				{
					if (icon.Item.HasTag(_other.UnstorableTags[j]))
					{
						flag = false;
						break;
					}
				}
				icon.IconMode = ((!flag) ? ItemIconWidget.Mode.Disabled : ItemIconWidget.Mode.Enabled);
			};
		}
		return null;
	}

	private string GetCurrentCategory()
	{
		if (_other == null || _other.Type != Durango.Logic.Item.Inventory.InventoryType.Warehouse)
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

	private void MultiselectMode(bool enable)
	{
		bool isMultiselect = IsMultiselect;
		if (enable)
		{
			_itemList.SelectableCount = -1;
			_multiselectButton.Selected = true;
			_menuBar.SetSelectAllButtonActive(activated: true);
		}
		else
		{
			_itemList.SelectableCount = 1;
			_multiselectButton.Selected = false;
			_titleWidget.Object.SetTitle(T._("가방"));
			_menuBar.SetSelectAllButtonActive(activated: false);
		}
		int i = 0;
		for (int size = KUtility.GetSize(_onlySingleSelectModeObject); i < size; i++)
		{
			if (_onlySingleSelectModeObject[i] != null)
			{
				_onlySingleSelectModeObject[i].SetActive(!enable);
			}
		}
		if (isMultiselect != enable)
		{
			_itemList.DeselectAllItems(sendEvent: true);
		}
	}

	protected virtual bool CloseAfterUsingItem()
	{
		return true;
	}

	private void Close()
	{
		UIBase.CloseAllUI();
	}

	private void OnUpdateSelectItem()
	{
		UpdateSelectedItemCount();
		if (_itemList.LastClickedItem == null)
		{
			ShowItemInfo(_itemList.FindIcon(_itemList.LastSelectedItem));
		}
		else
		{
			ItemIconWidget itemIconWidget = _itemList.FindIcon(_itemList.LastClickedItem);
			ShowItemInfo((!(itemIconWidget != null)) ? _itemList.FindIcon(_itemList.LastSelectedItem) : itemIconWidget);
		}
		_menuBar.SetSelectAllButtonSelected(IsEveryItemSelected);
	}

	private void UpdateSelectedItemCount()
	{
		int count = _itemList.SelectedList.Count;
		if (IsMultiselect)
		{
			_titleWidget.Object.SetTitle((count <= 0) ? T._("선택") : T._("<em>{0}개</em> 선택", count));
		}
	}

	protected void Refresh()
	{
		_isDirty = true;
	}

	private void LateRefresh()
	{
		_isDirty = false;
		_player = GameSystem<InventorySystem>.Instance().PlayerInventory;
		_other = GameSystem<InventorySystem>.Instance().TrackingInventory;
		if (_other != null && string.IsNullOrEmpty(_other.OwnerId))
		{
			_other = null;
		}
		if (_selectedTab == -1)
		{
			_selectedTab = 0;
			if (_inventoryMode == Durango.Logic.Item.Inventory.InventoryMode.Exchange && _other != null && (_other.Type == Durango.Logic.Item.Inventory.InventoryType.Artifact || _other.Type == Durango.Logic.Item.Inventory.InventoryType.Pet))
			{
				_selectedTab = 1;
			}
		}
		if (_prevSelectedTab != _selectedTab)
		{
			_prevSelectedTab = _selectedTab;
			Durango.Logic.Item.Inventory currentInventory = GetCurrentInventory();
			if (currentInventory != null)
			{
				switch (currentInventory.Type)
				{
				case Durango.Logic.Item.Inventory.InventoryType.Artifact:
				case Durango.Logic.Item.Inventory.InventoryType.Pet:
					currentInventory.Request();
					break;
				case Durango.Logic.Item.Inventory.InventoryType.Warehouse:
					GameSystem<InventorySystem>.Instance().GetWarehouseCategory(_selectedTab - 1);
					break;
				}
			}
		}
		UpdateInventoryInfo();
		UpdateTabList();
		UpdateItemList();
		UpdateSelectedItemCount();
	}

	private void EquipmentsUpdated()
	{
		_actionButtons.UpdateUseButtonAction(GetUsableActions(), _itemList.SelectedList);
	}

	private void UpdateInventoryInfo()
	{
		Durango.Logic.Item.Inventory currentInventory = GetCurrentInventory();
		int num = currentInventory?.CurrentSize() ?? 0;
		int num2 = currentInventory?.Capacity ?? 1;
		string arg = NGUIText.EncodeColor(num.ToString("0"), (num <= num2) ? Color.white : Color.red);
		if (!(_inventorySizeIcon == null) && !(_textInventorySize == null))
		{
			_inventorySizeIcon.fillAmount = ((num <= 0) ? 0f : (0.09f + 0.85f * ((float)num / (float)num2)));
			_textInventorySize.text = $"{arg} [3f3f3f]/[-] {num2:0}";
			UIUtility.UpdateAnchors(_textInventorySize.transform.parent);
		}
	}

	private void UpdateItemList()
	{
		TagPriorityComparer comparer = null;
		if (_inventoryMode == Durango.Logic.Item.Inventory.InventoryMode.Exchange && _other != null && (KUtility.GetSize(_other.StorableTags) > 0 || KUtility.GetSize(_other.UnstorableTags) > 0))
		{
			comparer = new TagPriorityComparer(_other.StorableTags, _other.UnstorableTags, Durango.Logic.Item.Util.GetItemComparison(_menuBar.SortOption), _menuBar.IsReversedSort);
		}
		_itemList.SetItemList(GetCurrentItemList(), FilterItem, GetCurrentInitFunc(), comparer);
	}

	private bool FilterItem([CanBeNull] ItemData item)
	{
		if (item == null)
		{
			return false;
		}
		return CheckBeingInCategory(item) && CheckContainingTag(item);
	}

	protected virtual void UpdateTabList()
	{
		if (_other != null || _pinnedTabList)
		{
			_tabList.gameObject.SetActive(value: true);
			ListObjectPool nodes = _tabList.Nodes;
			nodes.BeginLoad();
			KeyValueLabel component = nodes.GetNext().GetComponent<KeyValueLabel>();
			component.Set(T._("가방"), null);
			if (_other != null)
			{
				if (_other.Type == Durango.Logic.Item.Inventory.InventoryType.Warehouse)
				{
					int i = 0;
					for (int size = KUtility.GetSize(_other.Categories); i < size; i++)
					{
						KeyValueLabel component2 = nodes.GetNext().GetComponent<KeyValueLabel>();
						component2.SetKey(_other.Categories[i].Key);
					}
					_warehouseTabConfigBtn.gameObject.SetActive(value: true);
					_warehouseTabAddBtn.gameObject.SetActive(_other.CategoryCapacity > KUtility.GetSize(_other.Categories));
				}
				else
				{
					KeyValueLabel component3 = nodes.GetNext().GetComponent<KeyValueLabel>();
					component3.SetKey(TargetInventoryName(_other, T._("알수없음")));
					_warehouseTabConfigBtn.gameObject.SetActive(value: false);
					_warehouseTabAddBtn.gameObject.SetActive(value: false);
				}
			}
			nodes.EndLoad();
			for (int j = 0; j < nodes.Count; j++)
			{
				KeyValueLabel component4 = nodes[j].GetComponent<KeyValueLabel>();
				Durango.Logic.Item.Inventory inventory = GetInventory(j);
				if (inventory != null && inventory.State == Durango.Logic.Item.Inventory.InventoryState.Loaded)
				{
					if (inventory.Type == Durango.Logic.Item.Inventory.InventoryType.Warehouse)
					{
						component4.SetValue($"{inventory.Categories[j - 1].Value:0} / {Yaml.Util.Singleton<Constants>.Instance.Warehouse.SectionSize:0}");
					}
					else if (inventory.Capacity > 0)
					{
						component4.SetValue($"{inventory.CurrentSize():0} / {inventory.Capacity:0}");
					}
				}
				else
				{
					component4.SetValue(null);
				}
				Selectable component5 = nodes[j].GetComponent<Selectable>();
				component5.Selected = j == _selectedTab;
			}
			for (int k = 0; k < nodes.Count; k++)
			{
				KeyValueLabel component6 = nodes[k].GetComponent<KeyValueLabel>();
				component6.UpdateLayout();
				UIUtility.UpdateAnchors(component6.transform);
			}
			_tabList.Reposition();
			if (_warehouseTabAddBtn.gameObject.activeSelf)
			{
				UIWidget node = _tabList.GetNode(_tabList.GetNodeCount() - 1);
				UIWidget component7 = _warehouseTabAddBtn.GetComponent<UIWidget>();
				component7.SetPosition(node.GetPosition(1f, 0f) + _tabList.Margin * Vector3.right, 0f, 0f);
			}
		}
		else
		{
			_tabList.gameObject.SetActive(value: false);
		}
	}

	private void OnTabSelect(GameObject obj)
	{
		int num = _tabList.Nodes.IndexOf(obj);
		if (_selectedTab != num)
		{
			_selectedTab = num;
			_itemList.DeselectAllItems(sendEvent: false);
			Refresh();
			_itemList.ResetPosition();
		}
	}

	protected string TargetInventoryName(Durango.Logic.Item.Inventory inventory, string unknownText = null)
	{
		if (inventory == null)
		{
			return unknownText;
		}
		switch (inventory.Type)
		{
		case Durango.Logic.Item.Inventory.InventoryType.Pet:
		{
			Messages.Pet? pet = Durango.Utils.Singleton<PetManager>.Instance().GetPet(inventory.OwnerId);
			if (pet.HasValue)
			{
				return pet.Value.GetPetName();
			}
			break;
		}
		case Durango.Logic.Item.Inventory.InventoryType.Artifact:
		{
			Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(inventory.OwnerId);
			if (artifact != null)
			{
				return artifact.LocalizedName;
			}
			break;
		}
		}
		return unknownText;
	}

	private bool GetSelectedItemsLockState()
	{
		int count = _itemList.SelectedList.Count;
		if (count == 0)
		{
			return false;
		}
		bool result = true;
		for (int i = 0; i < count; i++)
		{
			if (!_itemList.SelectedList[i].Locked)
			{
				result = false;
			}
		}
		return result;
	}

	private void ShowItemInfo(ItemIconWidget itemIcon)
	{
		Durango.Logic.Item.Inventory currentInventory = GetCurrentInventory();
		if (itemIcon == null)
		{
			_itemInfo.Hide();
			_buttonContainer.SetActive(value: false);
			_menuBar.ItemLockEnable(on: false);
			_menuBar.ItemRemoveEnable(on: false);
			return;
		}
		string warnigText = null;
		if (itemIcon.IconMode != 0 && _selectedTab == 0 && _other != null)
		{
			if (KUtility.GetSize(_other.StorableTags) > 0)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < _other.StorableTags.Length; i++)
				{
					string text = _other.StorableTags[i];
					if (SingletonDict<string, Yaml.Tag>.TryGetValue(text, out var value))
					{
						list.Add(value.Name);
					}
					else if (Debug.isDebugBuild)
					{
						list.Add(text);
					}
				}
				if (list.Count > 0)
				{
					warnigText = T._("[icon=icon_make_alert] {0:l:{}|, } 속성이 없는 아이템은 옮길 수 없습니다.", list, list.Count);
				}
			}
			else if (KUtility.GetSize(_other.UnstorableTags) > 0)
			{
				List<string> list2 = new List<string>();
				for (int j = 0; j < _other.UnstorableTags.Length; j++)
				{
					string text2 = _other.UnstorableTags[j];
					if (SingletonDict<string, Yaml.Tag>.TryGetValue(text2, out var value2))
					{
						list2.Add(value2.Name);
					}
					else if (Debug.isDebugBuild)
					{
						list2.Add(text2);
					}
				}
				if (list2.Count > 0)
				{
					warnigText = T._("[icon=icon_make_alert] {0:l:{}|, } 속성을 갖는 아이템은 옮길 수 없습니다.", list2, list2.Count);
				}
			}
		}
		_itemInfo.Show(itemIcon.Item, warnigText);
		_buttonContainer.SetActive(value: true);
		_actionButtons.UpdateUseButtonAction(GetUsableActions(), _itemList.SelectedList);
		bool flag = false;
		if (currentInventory != null && currentInventory == _player)
		{
			flag = true;
		}
		if (flag)
		{
			bool selectedItemsLockState = GetSelectedItemsLockState();
			_menuBar.SetLockButtonSelection(selectedItemsLockState);
		}
		_menuBar.ItemRemoveEnable(on: true);
		_menuBar.ItemLockEnable(flag);
	}

	private void CheckValidItemInfo()
	{
		UpdateSelectedItemCount();
		_menuBar.SetSelectAllButtonSelected(IsEveryItemSelected);
		if (_itemList.IndexOf(_itemInfo.Item) == -1)
		{
			ItemData itemData = _itemList.LastClickedItem ?? _itemList.LastClickedItem;
			if (itemData == null || !_itemList.SelectedList.Contains(itemData))
			{
				itemData = _itemList.SelectedList.FirstOrDefault();
			}
			ShowItemInfo(_itemList.FindIcon(itemData));
		}
	}

	private string UseTypeToString(UseType type)
	{
		switch (type)
		{
		case UseType.PutIn:
		{
			if (_other != null && _other.Type == Durango.Logic.Item.Inventory.InventoryType.Warehouse)
			{
				return T._("옮기기");
			}
			string text = TargetInventoryName(_other);
			return (!string.IsNullOrEmpty(text)) ? T._("{0:으로} 옮기기", text) : type.GetName();
		}
		case UseType.TakeOut:
			if (_other != null && _other.Type == Durango.Logic.Item.Inventory.InventoryType.Warehouse)
			{
				return T._("옮기기");
			}
			break;
		}
		return type.GetName();
	}

	public void SetInventoryMode(Durango.Logic.Item.Inventory.InventoryMode mode)
	{
		_inventoryMode = mode;
		if (NGUITools.GetActive(this))
		{
			switch (_inventoryMode)
			{
			case Durango.Logic.Item.Inventory.InventoryMode.Normal:
				_tabList.gameObject.SetActive(value: false);
				MultiselectMode(enable: false);
				break;
			case Durango.Logic.Item.Inventory.InventoryMode.Dead:
				_tabList.gameObject.SetActive(value: false);
				MultiselectMode(enable: true);
				break;
			case Durango.Logic.Item.Inventory.InventoryMode.Exchange:
				_tabList.gameObject.SetActive(value: true);
				MultiselectMode(enable: true);
				break;
			}
			Refresh();
		}
	}

	private void OnUseItem(UseType useType)
	{
		if (_itemList.SelectedList.Count != 0)
		{
			List<ItemData> selectedList = _itemList.SelectedList;
			ItemAction(useType, selectedList);
		}
	}

	private void ItemAction(UseType type, IList<ItemData> items)
	{
		if (KUtility.GetSize(items) != 0)
		{
			switch (type)
			{
			case UseType.TakeOut:
				TakeOut(items);
				break;
			case UseType.PutIn:
				PutIn(items);
				break;
			case UseType.Eat:
			case UseType.Drink:
			case UseType.Ticket:
			case UseType.GainRecipes:
			case UseType.OpenBox:
			case UseType.Use:
				UseItem(items);
				break;
			case UseType.Imprint:
				Imprint(items);
				break;
			case UseType.ChangeDisplay:
				ChangeDisplay(items);
				break;
			case UseType.Equip:
				GameSystem<EquipSystem>.Instance().EquipItem(items.First());
				break;
			case UseType.UnEquip:
				GameSystem<EquipSystem>.Instance().EquipItem(items.First());
				break;
			case UseType.ResurrectionRewards:
				ResurrectionRewards(items);
				break;
			case UseType.Place:
				Place(items);
				break;
			case UseType.Repair:
				UIManager.FindScript<RepairGroup>().Open(items.First());
				break;
			case UseType.Build:
				Build(items);
				break;
			case UseType.Dye:
			{
				ItemData itemData2 = items.First();
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.Append("it_color");
				stringBuilder2.AppendFormat(" {0}", itemData2.Id);
				Connections.Frontend.Send(new Cheat
				{
					_Cheat = stringBuilder2.ToString().Trim()
				});
				break;
			}
			case UseType.Grazing:
			{
				ItemData itemData = items.First();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("pet_imprint");
				stringBuilder.AppendFormat(" {0}", itemData.Id);
				Connections.Frontend.Send(new Cheat
				{
					_Cheat = stringBuilder.ToString().Trim()
				});
				break;
			}
			case UseType.Taming:
			case UseType.Drop:
				break;
			}
		}
	}

	private void TakeOut(IList<ItemData> items)
	{
		if (_other == null)
		{
			return;
		}
		string[] movableItems = Durango.Logic.Item.Util.ItemsToIds(items);
		Durango.Logic.Item.Inventory.InventoryType type = _other.Type;
		switch (type)
		{
		case Durango.Logic.Item.Inventory.InventoryType.Artifact:
			InventorySystem.TakeOutItems(_other.OwnerId, _other.OwnerPosition, movableItems);
			break;
		case Durango.Logic.Item.Inventory.InventoryType.Pet:
			InventorySystem.TakeOutItemsFromPet(_other.OwnerId, movableItems);
			break;
		case Durango.Logic.Item.Inventory.InventoryType.Warehouse:
		{
			string current = GetCurrentCategory();
			if (KUtility.GetSize(_other.Categories) <= 1)
			{
				InventorySystem.TakeOutItemsFromWarehouse(_other.OwnerId, _other.OwnerPosition, current, movableItems);
				break;
			}
			int requireSize = items.Sum((ItemData elem) => elem.Size);
			_warehouseTabSelector.Set(_other, true, delegate(string category)
			{
				if (category == null)
				{
					InventorySystem.TakeOutItemsFromWarehouse(_other.OwnerId, _other.OwnerPosition, current, movableItems);
				}
				else
				{
					InventorySystem.MoveToItemsFromWarehouse(_other.OwnerId, _other.OwnerPosition, current, category, movableItems);
				}
			}, requireSize, current);
			_warehouseTabSelector.Show();
			break;
		}
		}
	}

	private void PutIn(IList<ItemData> items)
	{
		if (_other == null)
		{
			return;
		}
		List<ItemData> movableItems = items.Where((ItemData elem) => !elem.Locked && !elem.IsEquipments).ToList();
		if (movableItems.Any((ItemData elem) => elem.SafeLevel == SafeLevel.Protected))
		{
			string mainText = T._("<em>임무</em> 수행에 필요한 아이템이 포함되어 있습니다. 이동하시겠습니까?");
			UIManager.MessageBox.Show(mainText, delegate(bool ok)
			{
				if (ok)
				{
					DoPutIn(movableItems, items.Count);
				}
			});
		}
		else
		{
			DoPutIn(movableItems, items.Count);
		}
	}

	private void DoPutIn(IList<ItemData> items, int totalCount)
	{
		string[] itemIds = Durango.Logic.Item.Util.ItemsToIds(items);
		switch (_other.Type)
		{
		case Durango.Logic.Item.Inventory.InventoryType.Artifact:
			DisplayPutInMessage(items, totalCount);
			InventorySystem.PutInItems(_other.OwnerId, _other.OwnerPosition, itemIds);
			break;
		case Durango.Logic.Item.Inventory.InventoryType.Pet:
			DisplayPutInMessage(items, totalCount);
			InventorySystem.PutInItemsIntoPet(_other.OwnerId, itemIds);
			break;
		case Durango.Logic.Item.Inventory.InventoryType.Warehouse:
		{
			int requireSize = items.Sum((ItemData elem) => elem.Size);
			switch (KUtility.GetSize(_other.Categories))
			{
			case 0:
			{
				string newKey = T._("새 탭");
				GameSystem<InventorySystem>.Instance().AddWarehouseCategory(newKey, delegate(bool success)
				{
					if (success)
					{
						DisplayPutInMessage(items, totalCount);
						InventorySystem.PutInItemsIntoWarehouse(_other.OwnerId, _other.OwnerPosition, newKey, itemIds);
					}
				});
				break;
			}
			case 1:
				DisplayPutInMessage(items, totalCount);
				InventorySystem.PutInItemsIntoWarehouse(_other.OwnerId, _other.OwnerPosition, _other.Categories[0].Key, itemIds);
				break;
			default:
				_warehouseTabSelector.Set(_other, false, delegate(string category)
				{
					DisplayPutInMessage(items, totalCount);
					InventorySystem.PutInItemsIntoWarehouse(_other.OwnerId, _other.OwnerPosition, category, itemIds);
				}, requireSize);
				_warehouseTabSelector.Show();
				break;
			}
			break;
		}
		}
	}

	private static void DisplayPutInMessage(IList<ItemData> movableItems, int originalItemsCount)
	{
		Durango.Logic.Item.Inventory trackingInventory = GameSystem<InventorySystem>.Instance().TrackingInventory;
		if (trackingInventory.State != 0 && !trackingInventory.OnlyTakeOut && originalItemsCount != 0 && Durango.Logic.Item.Inventory.CheckEnableUseType(movableItems, UseType.PutIn))
		{
			if (movableItems.Count == 0)
			{
				UIManager.SystemMsg(T._("<em>착용</em> 중이거나 <em>잠긴</em> 물품은 이동할 수 없습니다."));
			}
			else if (movableItems.Count != originalItemsCount)
			{
				UIManager.SystemMsg(T._("<em>착용</em> 중이거나 <em>잠긴</em> 물품을 제외하고 이동하였습니다."));
			}
		}
	}

	private void UseItem(IList<ItemData> items)
	{
		ItemData firstItem = items.First();
		if (firstItem.PrototypeId == "skill_reset_ticket")
		{
			MessageBox messageBox = UIManager.MessageBox;
			string text = string.Format("[icon=icon_sp] <em>{0}</em> / {1}    [preset=animation_arrow]    [icon=icon_sp] <em>{1}</em> / {1}", GameSystem<SkillSystem>.Instance().RemainSkillPoint, GameSystem<SkillSystem>.Instance().SkillPoint);
			messageBox.AddKeyValueInfo(T._("스킬 포인트"), text);
			string mainText = T._("스킬 초기화권을 사용하시겠습니까?");
			string subText = T._("<alert><alert_icon/> 모든 스킬이 초기화됩니다.</alert>");
			messageBox.Show(mainText, subText, delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<InventorySystem>.Instance().UseItem(firstItem);
					if (CloseAfterUsingItem())
					{
						Close();
					}
				}
			}, T._("사용"));
			return;
		}
		UIManager.MessageBox.ShowLockConfirm(firstItem, delegate
		{
			GameSystem<InventorySystem>.Instance().UseItem(firstItem);
			if (CloseAfterUsingItem())
			{
				Close();
			}
		});
	}

	private static void Imprint(IList<ItemData> items)
	{
		ItemData reinItem = items.First();
		UIManager.MessageBox.ShowLockConfirm(reinItem, delegate
		{
			DoImprinting(reinItem);
		});
	}

	private static void DoImprinting(ItemData reinItem)
	{
		if (!reinItem.Reins.HasValue)
		{
			return;
		}
		Reins value = reinItem.Reins.Value;
		Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(value.PetEntityType);
		string path = ((pet != null) ? AnimalYaml.GetPrefabPath(pet.VehicleEntityType) : null);
		MessageBox messageBox = UIManager.MessageBox;
		UIWidget modelViewer = messageBox.ModelViewer;
		UIModelViewer componentInChildren = modelViewer.GetComponentInChildren<UIModelViewer>(includeInactive: true);
		componentInChildren.SetPlainModel(path, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = 140f,
			Loaded = componentInChildren.DefaultAnimalPlay("idle", "stand")
		});
		messageBox.SetCustomWidget(modelViewer, MessageBox.Position.Top);
		Messages.Pet? pet2 = value.Pet;
		string petName = (pet2.HasValue ? value.Pet.Value.GetPetName() : reinItem.Name);
		Messages.Pet? pet3 = value.Pet;
		string entityId = (pet3.HasValue ? value.Pet.Value.EntityId : null);
		messageBox.Show(T._("<em>{0}</em>{0:-을} 귀속하시겠습니까?", petName), T._("[icon=icon_make_alert] 한 번 귀속한 동물은 귀속해제 전까지 판매하거나 다른 사람에게 양도할 수 없습니다."), delegate(int index)
		{
			if (index == 0)
			{
				GameSystem<InventorySystem>.Instance().UseItem(reinItem, playerAccepted: false, delegate
				{
					SoundManager.PlayEvent("ui_button_animal_bind");
					UIManager.Alarm.ShowNotify(T._("{0:을} 귀속했습니다!", petName), "act_domesticate_1", major: true);
					PetGroup petGroup = UIManager.FindScript<PetGroup>();
					if (!(petGroup == null))
					{
						petGroup.Open(entityId);
					}
				});
			}
		}, new MessageBox.Button
		{
			Text = T._("네"),
			Style = PresetButton.Style.Solid
		}, new MessageBox.Button
		{
			Text = T._("아니오"),
			Style = PresetButton.Style.Border
		});
	}

	private static void ChangeDisplay(IList<ItemData> items)
	{
		ItemData ticket = items.First();
		EditPlayerDisplayGroup editPlayerDisplayGroup = UIManager.FindScript<EditPlayerDisplayGroup>();
		editPlayerDisplayGroup.OpenEditPlayerCostume(ticket);
	}

	private void ResurrectionRewards(IList<ItemData> items)
	{
		UIManager.MessageBox.ShowLockConfirm(items, delegate(string[] elem)
		{
			GameSystem<InventorySystem>.Instance().SetResurrectionReward(elem);
			Close();
		});
	}

	private static void Place(IList<ItemData> items)
	{
		ItemData item = items.First();
		UIManager.MessageBox.ShowLockConfirm(item, delegate
		{
			DoPlaceItem(item);
		});
	}

	private static void DoPlaceItem(ItemData item)
	{
		if (item.Capsule.HasValue)
		{
			ArtifactCapsule value = item.Capsule.Value;
			Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(value.BlueprintId);
			UIManager.FindScript<BuildGridGroupBase>().Open(blueprint, value.OccupySize, null, hasRoof: true, value.Display, delegate(BuildSystem.GridResult result)
			{
				string icon = ((result.Blueprint != null) ? result.Blueprint.Icon : null);
				BuildSystem.PlaceCapsulatedArtifact(item.Id, icon, result.Tile, result.Floor, result.Size, result.Rotation);
			});
		}
	}

	private static void Build(IList<ItemData> items)
	{
		ItemData item = items.First();
		UIManager.MessageBox.ShowLockConfirm(item, delegate
		{
			DoBuildItem(item);
		});
	}

	private static void DoBuildItem(ItemData item)
	{
		if (!item.Blueprint.HasValue)
		{
			return;
		}
		string blueprintId = item.Blueprint.Value.BlueprintId;
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(blueprintId);
		UIManager.FindScript<BuildGridGroupBase>().Open(blueprint, delegate(BuildSystem.GridResult result)
		{
			UIManager.MessageBox.Show(T._("<em>도면 아이템</em> 이 사라집니다. 건설 부지 선택을 완료하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<BuildSystem>.Instance().OccupyArtifactSite(result, item.Id);
				}
			});
		});
	}

	private void OnLockItem()
	{
		if (KUtility.GetSize(_itemList.SelectedList) != 0)
		{
			bool selectedItemsLockState = GetSelectedItemsLockState();
			selectedItemsLockState = !selectedItemsLockState;
			GameSystem<InventorySystem>.Instance().LockItem(selectedItemsLockState, Durango.Logic.Item.Util.ItemsToIds(_itemList.SelectedList));
			_menuBar.SetLockButtonSelection(selectedItemsLockState);
		}
	}

	private void OnSelectAllItem()
	{
		MultiselectMode(enable: true);
		if (!_itemList.IsAllItemsSelected)
		{
			_itemList.SelectAllItems();
		}
		else
		{
			_itemList.DeselectAllItems(sendEvent: true);
		}
	}

	private void AskDropSelectItem()
	{
		if (KUtility.GetSize(_itemList.SelectedList) == 0)
		{
			return;
		}
		List<ItemData> selectedList = _itemList.SelectedList;
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<ItemData> list = new List<ItemData>(selectedList.Count);
		foreach (ItemData item in selectedList)
		{
			if (item.IsEquipments)
			{
				num2++;
				continue;
			}
			if (item.Locked)
			{
				num++;
				continue;
			}
			if (!item.Dumpable)
			{
				num3++;
				continue;
			}
			if (!item.Tradable)
			{
				flag = true;
			}
			if (item.SafeLevel == SafeLevel.Protected)
			{
				flag2 = true;
			}
			list.Add(item);
		}
		if (list.Count == 0)
		{
			string comment = ((num2 == selectedList.Count) ? T._("<em>착용</em> 중인 아이템은 버릴 수 없습니다.") : ((num == selectedList.Count) ? T._("<em>잠긴</em> 아이템은 버릴 수 없습니다.") : ((num3 != selectedList.Count) ? T._("<em>착용</em> 중이거나 <em>잠긴</em> 물품은 버릴 수 없습니다.") : T._("버릴 수 없는 아이템입니다."))));
			UIManager.SystemMsg(comment);
			return;
		}
		string warningMessage = ((!flag) ? T._("<alert_icon/> 버린 아이템은 시간이 지나면 사라집니다.") : T._("<alert><alert_icon/> 거래할 수 없는 아이템은 버리면 다시 주울 수 없습니다.</alert>"));
		if (list.Count < selectedList.Count)
		{
			warningMessage += T._("\n<alert_icon/> <em>착용</em> 중이거나 <em>잠긴</em> 물품은 제외됩니다.");
		}
		if (flag2)
		{
			warningMessage += T._("\n<alert><alert_icon/> 임무 수행에 필요한 아이템이 포함되어 있습니다.</alert>");
		}
		DumpItems dumpItems = InventorySystem.MakeDumpItemsPacket(GetCurrentInventory(), Durango.Logic.Item.Util.ItemsToIds(list));
		UIManager.MessageBox.Show((!flag) ? T._("아이템을 버리시겠습니까?") : T._("거래불가 아이템을 버리시겠습니까?"), warningMessage, delegate(int index)
		{
			if (index != 2 && GameManager.Region.Role() == Role.Personal && !Preferences.GetBool("dropped_item_in_personal_region"))
			{
				UIManager.MessageBox.Show(T._("아이템을 정말 버리시겠습니까?"), T._("\n<alert><alert_icon/> 개인섬에서도 버려진 물건은 내구도가 감소되어 사라집니다.</alert>"), delegate(bool ok)
				{
					if (ok)
					{
						Preferences.SetBool("dropped_item_in_personal_region", value: true);
						DropConfirmed(index, dumpItems, warningMessage);
					}
				});
			}
			else
			{
				DropConfirmed(index, dumpItems, warningMessage);
			}
		}, new MessageBox.Button(T._("버리기")), new MessageBox.Button(T._("장소 선택하여 버리기")), T._("취소"));
	}

	private static void DropConfirmed(int index, DumpItems dumpItems, string warningMessage)
	{
		switch (index)
		{
		case 0:
			InventorySystem.DropItems(dumpItems);
			SoundManager.PlayEvent("ui_item_dump");
			break;
		case 1:
			DropToSpecificTile(dumpItems, warningMessage);
			break;
		}
	}

	private static void DropToSpecificTile(DumpItems dumpItems, string warningMessage)
	{
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint("package");
		BuildLocator.Arguments args;
		if (blueprint == null)
		{
			BuildLocator.Arguments arguments = default(BuildLocator.Arguments);
			arguments.Size = Point2.one;
			arguments.Exterior = true;
			args = arguments;
		}
		else
		{
			args = BuildLocator.Arguments.MakeFrom(blueprint);
		}
		args.RotatableDirections = 0;
		args.Dump = true;
		args.Interior = false;
		UIManager.FindScript<BuildGridGroupBase>().Open(new BuildGridGroupBase.Arguments
		{
			Comment = T._("아이템을 버릴 장소를 선택하세요."),
			Args = args,
			Confirmed = delegate(BuildSystem.GridResult result)
			{
				dumpItems.Tile = result.Tile;
				dumpItems.Floor = result.Floor;
				Action action = delegate
				{
					InventorySystem.DropItems(dumpItems);
					SoundManager.PlayEvent("ui_item_dump");
				};
				EstateInfo estateInfo = EstateSystem.GetEstateInfo(result.Tile);
				if (!DropToEstate(estateInfo, warningMessage, action))
				{
					action();
				}
			}
		});
	}

	private static bool DropToEstate(EstateInfo estate, string warningMessage, Action drop)
	{
		if (estate == null)
		{
			return false;
		}
		switch (estate.License.Type)
		{
		case OwnerType.Player:
		case OwnerType.PersonalPlayer:
			if (!(estate.License.OwnerId != GameManager.PlayerId))
			{
				break;
			}
			Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(estate.License.OwnerId, delegate(Durango.Player.PlayerInfo info)
			{
				MessageBox messageBox = UIManager.MessageBox;
				messageBox.Show(T._("<em>{0}</em> 님의 사유지에 아이템을 버리시겠습니까?", info.GetNameFreq(21, string.Empty)), warningMessage, delegate(bool ok)
				{
					if (ok)
					{
						drop();
					}
				});
			});
			return true;
		case OwnerType.ClanEstate:
		case OwnerType.ClanWarphole:
			if (ClanSystem.IsMyClan(estate.License.OwnerId))
			{
				break;
			}
			ClanSystem.GetClanInfo(estate.License.OwnerId, delegate(Clan clan)
			{
				MessageBox messageBox2 = UIManager.MessageBox;
				messageBox2.Show(T._("<em>{0}</em> 부족의 영토에 아이템을 버리시겠습니까?", (clan != null) ? clan.Name : string.Empty), warningMessage, delegate(bool ok)
				{
					if (ok)
					{
						drop();
					}
				});
			});
			return true;
		}
		return false;
	}

	private void OnSortItemList(Durango.Logic.Item.Util.SortOption option, bool descending)
	{
		Durango.Logic.Item.Inventory currentInventory = GetCurrentInventory();
		if (currentInventory != null)
		{
			SoundManager.PlayEvent(_rearrangeSound);
			Durango.Logic.Item.Util.SortItems(currentInventory.Items, option, descending);
			GameSystem<InventorySystem>.Instance().SendItemLocationInfo(currentInventory);
			_itemList.SetItemList(currentInventory.Items, FilterItem);
		}
	}

	private void OnFilterItem()
	{
		Durango.Logic.Item.Inventory currentInventory = GetCurrentInventory();
		if (currentInventory != null)
		{
			if (IsFilterApplied)
			{
				_tagFilters.Clear();
				SoundManager.PlayEvent(_rearrangeSound);
				_itemList.SetItemList(currentInventory.Items, CheckBeingInCategory);
				_menuBar.ItemFilterSelection(on: false);
			}
			else
			{
				TagSelectPopup tagSelectPopup = UIManager.Popup.Tooltip<TagSelectPopup>();
				tagSelectPopup.Show();
				tagSelectPopup.Set(_tagFilters.ToList(), ApplyTagsFilter, Durango.Logic.Item.Util.SelectManyTags(currentInventory.Items));
			}
		}
	}

	private void ApplyCategoryFilter([NotNull] IEnumerable<Category.Main> selectedCategories)
	{
		Durango.Logic.Item.Inventory currentInventory = GetCurrentInventory();
		if (currentInventory != null)
		{
			SoundManager.PlayEvent(_rearrangeSound);
			_categoryFilters.Clear();
			_categoryFilters.AddRange(selectedCategories);
			_itemList.SetItemList(currentInventory.Items, FilterItem);
		}
	}

	private void ApplyTagsFilter(IEnumerable<string> selectedTags)
	{
		Durango.Logic.Item.Inventory currentInventory = GetCurrentInventory();
		if (currentInventory != null)
		{
			SoundManager.PlayEvent(_rearrangeSound);
			_tagFilters.Clear();
			_tagFilters.AddRange(selectedTags);
			_itemList.SetItemList(currentInventory.Items, FilterItem);
			_menuBar.ItemFilterSelection(IsFilterApplied);
		}
	}

	private bool CheckBeingInCategory(ItemData item)
	{
		if (_categoryFilters == null || _categoryFilters.Count == 0)
		{
			return true;
		}
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(item.PrototypeId, item.Level);
		if (itemPrototype == null)
		{
			return false;
		}
		string category = itemPrototype.Category;
		foreach (Category.Main categoryFilter in _categoryFilters)
		{
			if (categoryFilter.Id == category)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckContainingTag(ItemData item)
	{
		if (_tagFilters == null || _tagFilters.Count == 0)
		{
			return true;
		}
		if (_tagFilters.Any((string elem) => item.Tags.All((TagData x) => x.Id != elem)))
		{
			return false;
		}
		return true;
	}

	private List<UseType> GetUsableActions()
	{
		Useable.FillUsable(_usableList, _itemList.SelectedList, GetCurrentInventory(), _other, _inventoryMode);
		_usableList.Sort();
		return _usableList;
	}
}

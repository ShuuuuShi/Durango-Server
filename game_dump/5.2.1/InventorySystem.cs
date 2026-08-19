using System;
using System.Collections.Generic;
using System.Text;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Terrain;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class InventorySystem : GameSystem<InventorySystem>
{
	private bool _inventoryInitialized;

	private readonly HashSet<string> _lockedItems = new HashSet<string>();

	private readonly HashSet<string> _protectedItems = new HashSet<string>();

	private readonly Durango.Logic.Item.Inventory _playerInventory = new Durango.Logic.Item.Inventory();

	private readonly Durango.Logic.Item.Inventory _trackingInventory = new Durango.Logic.Item.Inventory();

	private Wallet? _wallet;

	private readonly Dictionary<string, Action<ItemData>> _onItemEvents = new Dictionary<string, Action<ItemData>>();

	public Durango.Logic.Item.Inventory PlayerInventory => _playerInventory;

	public List<ItemData> PlayerItemList => _playerInventory.Items;

	public Durango.Logic.Item.Inventory TrackingInventory => _trackingInventory;

	public static Wallet Wallet
	{
		get
		{
			if (!GameSystem<InventorySystem>.HasInstance())
			{
				return default(Wallet);
			}
			Wallet? wallet = GameSystem<InventorySystem>.Instance()._wallet;
			if (wallet.HasValue)
			{
				return wallet.Value;
			}
			return default(Wallet);
		}
	}

	public event Action PlayerInventoryUpdated;

	public event Action<Currency, long, long> BalanceChanged;

	public event Action WalletUpdated;

	public event Action<ItemExpired> PlayerItemExpired;

	public event Action TrackingInventoryUpdated;

	public event Action<ItemData, StartTimer> UseItemStarted;

	public event Action<ItemData> UseItemSucceed;

	public event Action<ItemData> ItemAdded;

	private void Awake()
	{
		Connections.Frontend.On<Messages.Inventory>(ReceiveInventoryMsg);
		Connections.Frontend.On<ItemUsed>(ReceiveItemUsedMsg);
		Connections.Frontend.On<ItemExpired>(ReceiveItemExpiredMsg);
		Connections.Frontend.On<Messages.Warehouse>(ReceiveWarehouseMsg);
		Connections.Frontend.On<PetInventory>(ReceivePetInventoryMsg);
		Connections.Frontend.On<InventoryUpdated>(ReceiveInventoryUpdated);
		Connections.Frontend.On<WarehouseUpdated>(ReceiveWareHouseUpdated);
		Connections.Frontend.On<InventoryInfos>(ReceiveInventoryInfosMsg);
		Connections.Frontend.On<InventoryItems>(ReceiveInventoryItemsMsg);
		Connections.Frontend.On<AskEatFoodOverrideStatusEffect>(ReceiveAskEatFoodOverrideStatusEffectMsg);
		Connections.Frontend.On<SectionUpdated>(ReceiveSectionUpdatedMsg);
		Connections.Frontend.On<WalletUpdated>(ReceiveWalletUpdated);
		Connections.Frontend.On<ProtectedItems>(ReceiveProtectedItemsMsg);
	}

	private void Start()
	{
		GameSystem<CraftSystem>.Instance().CraftSucceed += OnCraftSucceed;
	}

	private void ReceiveInventoryMsg(Messages.Inventory msg, PacketHeader header)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			UpdatePlayerInventoryInfo(msg.InventoryInfos);
			UpdatePlayerInventoryItem(msg.InventoryItems);
			UpdatePlayerWallet(msg.Wallet);
			_inventoryInitialized = true;
			OnUpdatePlayerInventory();
		}
		else if (_trackingInventory.OwnerId == msg.EntityId)
		{
			UpdateTrackingInventory(msg);
		}
	}

	private void ReceiveItemExpiredMsg(ItemExpired msg, PacketHeader header)
	{
		if (this.PlayerItemExpired != null)
		{
			this.PlayerItemExpired(msg);
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		foreach (KeyValuePair<string, string> itemName in msg.ItemNames)
		{
			value.Append(T._("<em>{0}</em>{0:-이} 내구도가 다 되어 파괴되었습니다.", itemName.Value)).AppendLine();
		}
		string text = value.ToString().Trim();
		if (!string.IsNullOrEmpty(text))
		{
			GameSystem<SocialSystem>.Instance().AddSystemChat(text, string.Empty);
		}
	}

	private void ReceiveInventoryInfosMsg(InventoryInfos msg, PacketHeader header)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			UpdatePlayerInventoryInfo(msg);
		}
	}

	private void ReceiveInventoryItemsMsg(InventoryItems msg, PacketHeader header)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			UpdatePlayerInventoryItem(msg);
			OnUpdatePlayerInventory();
		}
	}

	private void ReceiveWalletUpdated(WalletUpdated msg, PacketHeader header)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			UpdatePlayerWallet(msg.Wallet);
			OnUpdatePlayerInventory();
		}
	}

	private void ReceiveProtectedItemsMsg(ProtectedItems msg, PacketHeader header)
	{
		UpdateProtectedItems(msg.ItemIds);
		UpdatePlayerItemsSafeLevel();
	}

	private void ReceiveInventoryUpdated(InventoryUpdated msg, PacketHeader header)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			if (msg.ProtectedItems.HasValue && msg.ProtectedItems.Value.ItemIds != null)
			{
				_protectedItems.AddRange(msg.ProtectedItems.Value.ItemIds);
			}
			AddPlayerInventoryItems(msg.Items);
			RemoveInventoryItems(_playerInventory, msg.RemovedItemIds);
			_playerInventory.SetOrder(msg.ItemOrder);
			OnUpdatePlayerInventory();
		}
		else if (msg.EntityId == _trackingInventory.OwnerId)
		{
			AddInventoryItems(_trackingInventory, msg.Items);
			RemoveInventoryItems(_trackingInventory, msg.RemovedItemIds);
			_trackingInventory.SetOrder(msg.ItemOrder);
			if (this.TrackingInventoryUpdated != null)
			{
				this.TrackingInventoryUpdated();
			}
		}
	}

	private void ReceiveWareHouseUpdated(WarehouseUpdated msg, PacketHeader header)
	{
		if (msg.EntityId != _trackingInventory.OwnerId || _trackingInventory.Type != Durango.Logic.Item.Inventory.InventoryType.Warehouse)
		{
			return;
		}
		List<KeyValuePair<string, int>> categories = _trackingInventory.Categories;
		int i = 0;
		for (int size = KUtility.GetSize(categories); i < size; i++)
		{
			if (!(categories[i].Key != msg.SectionName))
			{
				categories[i] = new KeyValuePair<string, int>(msg.SectionName, msg.UsedSize);
				break;
			}
		}
		if (msg.SectionName == _trackingInventory.SelectedCategory)
		{
			AddInventoryItems(_trackingInventory, msg.Items);
			RemoveInventoryItems(_trackingInventory, msg.RemovedItemIds);
		}
		if (this.TrackingInventoryUpdated != null)
		{
			this.TrackingInventoryUpdated();
		}
	}

	private void ReceiveWarehouseMsg(Messages.Warehouse msg, PacketHeader header)
	{
		if (msg.EntityId == _trackingInventory.OwnerId)
		{
			UpdateTrackingInventory(msg);
		}
	}

	private void ReceivePetInventoryMsg(PetInventory msg, PacketHeader header)
	{
		if (msg.Inven.EntityId == _trackingInventory.OwnerId)
		{
			UpdateTrackingInventory(msg.Inven);
		}
	}

	private void ReceiveSectionUpdatedMsg(SectionUpdated msg, PacketHeader header)
	{
		if (msg.EntityId == _trackingInventory.OwnerId)
		{
			UpdateSection(msg);
		}
	}

	private void UpdatePlayerInventoryInfo(InventoryInfos inventoryInfos)
	{
		_playerInventory.Capacity = inventoryInfos.MaxSize;
		UpdateLockedItems(inventoryInfos.LockedItemIds);
		UpdateProtectedItems(inventoryInfos.ProtectedItems.ItemIds);
		UpdatePlayerItemsSafeLevel();
		OnUpdatePlayerInventory();
	}

	private void OnUpdatePlayerInventory()
	{
		_playerInventory.Requested();
		if (this.PlayerInventoryUpdated != null)
		{
			this.PlayerInventoryUpdated();
		}
	}

	private void UpdatePlayerInventoryItem(InventoryItems inventoryItems)
	{
		List<ItemData> items = _playerInventory.Items;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			items[i].Valid = false;
		}
		int j = 0;
		for (int num = inventoryItems.Items.Length; j < num; j++)
		{
			AddPlayerInventoryItem(inventoryItems.Items[j]);
		}
		for (int num2 = items.Count - 1; num2 >= 0; num2--)
		{
			if (!items[num2].Valid)
			{
				items.RemoveAt(num2);
			}
		}
	}

	[CanBeNull]
	private ItemData AddInventoryItem(Durango.Logic.Item.Inventory inventory, Item msgItem, out bool isNew)
	{
		if (string.IsNullOrEmpty(msgItem.Id))
		{
			isNew = false;
			return null;
		}
		List<ItemData> items = inventory.Items;
		int num = -1;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			if (items[i].Id == msgItem.Id)
			{
				num = i;
				break;
			}
		}
		isNew = false;
		ItemData itemData;
		if (num == -1)
		{
			itemData = new ItemData(msgItem);
			items.Add(itemData);
			isNew = true;
			Action<ItemData> action = _onItemEvents.Get(itemData.Id);
			if (action != null)
			{
				action(itemData);
				_onItemEvents.Remove(itemData.Id);
			}
		}
		else
		{
			itemData = items[num];
			itemData.Set(msgItem);
		}
		return itemData;
	}

	private void AddInventoryItems(Durango.Logic.Item.Inventory inventory, Item[] items)
	{
		if (KUtility.GetSize(items) != 0)
		{
			for (int i = 0; i < items.Length; i++)
			{
				AddInventoryItem(inventory, items[i], out var _);
			}
		}
	}

	private void AddPlayerInventoryItem(Item msgItem)
	{
		bool isNew;
		ItemData itemData = AddInventoryItem(_playerInventory, msgItem, out isNew);
		if (itemData == null)
		{
			return;
		}
		itemData.Valid = true;
		itemData.IsEquipments = GameSystem<EquipSystem>.Instance().IsEquippedItem(itemData);
		UpdateItemSafeLevel(itemData);
		if (isNew && _inventoryInitialized)
		{
			itemData.IsNew = true;
			if (this.ItemAdded != null)
			{
				this.ItemAdded(itemData);
			}
		}
	}

	private void AddPlayerInventoryItems(Item[] items)
	{
		if (KUtility.GetSize(items) != 0)
		{
			for (int i = 0; i < items.Length; i++)
			{
				AddPlayerInventoryItem(items[i]);
			}
		}
	}

	private void RemoveInventoryItems(Durango.Logic.Item.Inventory inventory, string[] ids)
	{
		List<ItemData> items = inventory.Items;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (Array.IndexOf(ids, items[num].Id) != -1)
			{
				items.RemoveAt(num);
			}
		}
	}

	private void UpdateLockedItems(string[] lockedItems)
	{
		_lockedItems.Clear();
		if (lockedItems != null)
		{
			_lockedItems.AddRange(lockedItems);
		}
	}

	private void UpdateProtectedItems(string[] protectedItems)
	{
		_protectedItems.Clear();
		if (protectedItems != null)
		{
			_protectedItems.AddRange(protectedItems);
		}
	}

	private void UpdatePlayerItemsSafeLevel()
	{
		foreach (ItemData item in _playerInventory.Items)
		{
			UpdateItemSafeLevel(item);
		}
	}

	private void UpdateItemSafeLevel(ItemData itemdata)
	{
		if (_lockedItems.Contains(itemdata.Id))
		{
			itemdata.SafeLevel = SafeLevel.Locked;
		}
		else if (_protectedItems.Contains(itemdata.Id))
		{
			itemdata.SafeLevel = SafeLevel.Protected;
		}
		else
		{
			itemdata.SafeLevel = SafeLevel.None;
		}
	}

	private void UpdatePlayerWallet(Wallet? wallet)
	{
		Wallet? wallet2 = _wallet;
		_wallet = wallet;
		Currency[] array = Enums<Currency>.All();
		if (wallet2.HasValue)
		{
			for (int i = 0; i < array.Length; i++)
			{
				long balance = wallet2.Value.GetBalance(array[i]);
				long num = ((!_wallet.HasValue) ? 0 : _wallet.Value.GetBalance(array[i]));
				if (num != balance && this.BalanceChanged != null)
				{
					this.BalanceChanged(array[i], num, num - balance);
				}
			}
		}
		if (this.WalletUpdated != null)
		{
			this.WalletUpdated();
		}
	}

	private void UpdateTrackingInventory(Messages.Inventory msg)
	{
		List<ItemData> items = _trackingInventory.Items;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			items[i].Valid = false;
		}
		int j = 0;
		for (int num = msg.InventoryItems.Items.Length; j < num; j++)
		{
			int num2 = -1;
			Item itemInfo = msg.InventoryItems.Items[j];
			if (string.IsNullOrEmpty(itemInfo.Id))
			{
				continue;
			}
			int k = 0;
			for (int count2 = items.Count; k < count2; k++)
			{
				if (items[k].Id == itemInfo.Id)
				{
					num2 = k;
					break;
				}
			}
			ItemData itemData;
			if (num2 == -1)
			{
				itemData = new ItemData(itemInfo);
				items.Add(itemData);
			}
			else
			{
				itemData = items[num2];
				itemData.Set(itemInfo);
			}
			itemData.Valid = true;
		}
		for (int num3 = items.Count - 1; num3 >= 0; num3--)
		{
			if (!items[num3].Valid)
			{
				items.RemoveAt(num3);
			}
		}
		_trackingInventory.Capacity = msg.InventoryInfos.MaxSize;
		_trackingInventory.Requested();
		if (this.TrackingInventoryUpdated != null)
		{
			this.TrackingInventoryUpdated();
		}
	}

	private void UpdateTrackingInventory(Messages.Warehouse warehouse)
	{
		_trackingInventory.Items.Clear();
		_trackingInventory.Capacity = Yaml.Util.Singleton<Constants>.Instance.Warehouse.SectionSize;
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(warehouse.SectionInfos.Length);
		int i = 0;
		for (int num = warehouse.SectionInfos.Length; i < num; i++)
		{
			InventorySectionInfos inventorySectionInfos = warehouse.SectionInfos[i];
			list.Add(new KeyValuePair<string, int>(inventorySectionInfos.SectionName, inventorySectionInfos.UsedSize));
		}
		_trackingInventory.Categories = list;
		_trackingInventory.CategoryCapacity = 10;
		_trackingInventory.SelectedCategory = null;
		_trackingInventory.Requested();
		if (this.TrackingInventoryUpdated != null)
		{
			this.TrackingInventoryUpdated();
		}
	}

	private void UpdateSection(SectionUpdated msg)
	{
		if (msg.Added.HasValue)
		{
			InventorySectionInfos value = msg.Added.Value;
			_trackingInventory.Categories.Add(new KeyValuePair<string, int>(value.SectionName, value.UsedSize));
		}
		else if (msg.Renamed.HasValue)
		{
			string key = msg.Renamed.Value.Item1;
			List<KeyValuePair<string, int>> categories = _trackingInventory.Categories;
			int num = categories.FindIndex((KeyValuePair<string, int> f) => f.Key == key);
			if (num != -1)
			{
				categories[num] = new KeyValuePair<string, int>(msg.Renamed.Value.Item2, categories[num].Value);
			}
		}
		else if (!string.IsNullOrEmpty(msg.Removed))
		{
			List<KeyValuePair<string, int>> categories2 = _trackingInventory.Categories;
			int num2 = categories2.FindIndex((KeyValuePair<string, int> f) => f.Key == msg.Removed);
			if (num2 != -1)
			{
				categories2.RemoveAt(num2);
			}
		}
		if (this.TrackingInventoryUpdated != null)
		{
			this.TrackingInventoryUpdated();
		}
	}

	public void AddOnItemEvent(string itemId, [NotNull] Action<ItemData> action)
	{
		ItemData itemData = _playerInventory.Find(itemId);
		if (itemData == null)
		{
			Action<ItemData> action2 = _onItemEvents.Get(itemId);
			action2 = ((action2 != null) ? ((Action<ItemData>)Delegate.Combine(action2, action)) : action);
			_onItemEvents[itemId] = action2;
		}
		else
		{
			action(itemData);
		}
	}

	public void GetWarehouseCategory(int index)
	{
		if (_trackingInventory.Type != Durango.Logic.Item.Inventory.InventoryType.Warehouse)
		{
			return;
		}
		int size = KUtility.GetSize(_trackingInventory.Categories);
		if (index < 0 || index >= size)
		{
			return;
		}
		string cat = _trackingInventory.Categories[index].Key;
		_trackingInventory.SelectedCategory = cat;
		_trackingInventory.Items.Clear();
		Connections.Frontend.Send(new GetSectionItems
		{
			EntityId = _trackingInventory.OwnerId,
			Tile = _trackingInventory.OwnerPosition,
			SectionName = cat
		}).On(delegate(SectionItems msg, PacketHeader header)
		{
			if (!(_trackingInventory.SelectedCategory != cat))
			{
				_trackingInventory.Items.Clear();
				int i = 0;
				for (int size2 = KUtility.GetSize(msg.Items); i < size2; i++)
				{
					if (!string.IsNullOrEmpty(msg.Items[i].Id))
					{
						_trackingInventory.Items.Add(new ItemData(msg.Items[i]));
					}
				}
				_trackingInventory.SetOrder(msg.ItemOrder);
				if (this.TrackingInventoryUpdated != null)
				{
					this.TrackingInventoryUpdated();
				}
			}
		});
	}

	public void SetResurrectionReward(string[] rewardIds)
	{
		Connections.Frontend.Send(new SetResurrectionRewards
		{
			ItemIds = rewardIds
		});
	}

	private void OnCraftSucceed(string recipeId, Crafted crafted)
	{
		int i = 0;
		for (int size = KUtility.GetSize(crafted.Items); i < size; i++)
		{
			ItemData itemData = FindItem(crafted.Items[i].Id);
			if (itemData != null)
			{
				itemData.IsNew = true;
			}
		}
	}

	public void SetArtifactInventory([NotNull] Artifact artifact, bool onlyTakeOut)
	{
		if (!(_trackingInventory.OwnerId == artifact.EntityId))
		{
			_trackingInventory.Reset();
			_trackingInventory.OwnerId = artifact.EntityId;
			_trackingInventory.OwnerPosition = artifact.WorldTile;
			_trackingInventory.OnlyTakeOut = onlyTakeOut;
			_trackingInventory.StorableTags = ((!artifact.ArtifactState.Inventory.HasValue) ? null : artifact.ArtifactState.Inventory.Value.StorableTags);
			_trackingInventory.UnstorableTags = ((!artifact.ArtifactState.Inventory.HasValue) ? null : artifact.ArtifactState.Inventory.Value.UnstorableTags);
			_trackingInventory.Type = Durango.Logic.Item.Inventory.InventoryType.Artifact;
		}
	}

	public void SetWarehouseInventory([NotNull] Artifact artifact)
	{
		if (!(_trackingInventory.OwnerId == artifact.EntityId))
		{
			_trackingInventory.Reset();
			_trackingInventory.OwnerId = artifact.EntityId;
			_trackingInventory.OwnerPosition = artifact.WorldTile;
			_trackingInventory.Type = Durango.Logic.Item.Inventory.InventoryType.Warehouse;
			_trackingInventory.Request();
		}
	}

	public void SetReinsInventory(string petId)
	{
		if (!(_trackingInventory.OwnerId == petId))
		{
			_trackingInventory.Reset();
			_trackingInventory.OwnerId = petId;
			_trackingInventory.OwnerPosition = -Point2.one;
			_trackingInventory.Type = Durango.Logic.Item.Inventory.InventoryType.Pet;
		}
	}

	private void ReceiveItemUsedMsg(ItemUsed m, PacketHeader header)
	{
		if (!string.IsNullOrEmpty(m.Motion))
		{
			PlayerController.MotionUpdater.Motion(m.Motion, m.Time);
		}
		if (!string.IsNullOrEmpty(m.Msg))
		{
			UIManager.SystemMsg(m.Msg, 4f);
		}
	}

	public void ChangeWarehouseCategoryName(string oldName, string newName)
	{
		if (_trackingInventory.Type == Durango.Logic.Item.Inventory.InventoryType.Warehouse)
		{
			Connections.Frontend.Send(new RenameWarehouseSection
			{
				EntityId = _trackingInventory.OwnerId,
				Tile = _trackingInventory.OwnerPosition,
				SectionName = oldName,
				NewName = newName
			});
		}
	}

	public void SetWarehouseCategoryList(string[] list)
	{
		if (_trackingInventory.Type == Durango.Logic.Item.Inventory.InventoryType.Warehouse)
		{
			Connections.Frontend.Send(new SetSectionOrder
			{
				EntityId = _trackingInventory.OwnerId,
				Tile = _trackingInventory.OwnerPosition,
				SectionOrder = list
			});
		}
	}

	public void AddWarehouseCategory(string key, Action<bool> onResult = null)
	{
		if (_trackingInventory.Type != Durango.Logic.Item.Inventory.InventoryType.Warehouse)
		{
			return;
		}
		Connections.Frontend.Send(new MakeSection
		{
			EntityId = _trackingInventory.OwnerId,
			Tile = _trackingInventory.OwnerPosition,
			SectionName = key
		}).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public void RemoveWarehouseCategory(string key)
	{
		if (_trackingInventory.Type != Durango.Logic.Item.Inventory.InventoryType.Warehouse)
		{
			return;
		}
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_trackingInventory.Categories); i < size; i++)
		{
			if (_trackingInventory.Categories[i].Key == key)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			UIManager.SystemMsg(T._("{0:을} 찾을 수 없습니다."), key);
			return;
		}
		if (_trackingInventory.Categories[num].Value > 0)
		{
			UIManager.SystemMsg(T._("{0:이} 비어있지 않습니다.", key));
			return;
		}
		Connections.Frontend.Send(new RemoveSection
		{
			EntityId = _trackingInventory.OwnerId,
			Tile = _trackingInventory.OwnerPosition,
			SectionName = key
		});
	}

	public void SendItemLocationInfo(Durango.Logic.Item.Inventory inventory)
	{
		if (inventory != null && KUtility.GetSize(inventory.Items) != 0)
		{
			List<ItemData> items = inventory.Items;
			switch (inventory.Type)
			{
			case Durango.Logic.Item.Inventory.InventoryType.Artifact:
				Connections.Frontend.Send(new InventoryOrder
				{
					TargetArtifact = new PropKey
					{
						EntityId = inventory.OwnerId,
						Tile = inventory.OwnerPosition
					},
					ItemOrder = Durango.Logic.Item.Util.ItemsToIds(items)
				});
				break;
			case Durango.Logic.Item.Inventory.InventoryType.Warehouse:
				Connections.Frontend.Send(new SetSectionItemOrder
				{
					EntityId = inventory.OwnerId,
					Tile = inventory.OwnerPosition,
					SectionName = inventory.SelectedCategory,
					ItemOrder = Durango.Logic.Item.Util.ItemsToIds(items)
				});
				break;
			default:
				Connections.Frontend.Send(new InventoryOrder
				{
					ItemOrder = Durango.Logic.Item.Util.ItemsToIds(items)
				});
				break;
			case Durango.Logic.Item.Inventory.InventoryType.Pet:
				break;
			}
		}
	}

	public void UseItem(ItemData item, bool playerAccepted = false, Action onSuccess = null)
	{
		Connections.Frontend.Send(new UseItem
		{
			ItemId = item.Id,
			Accept = playerAccepted
		}).On(delegate(StartTimer msg, PacketHeader header)
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
			if (this.UseItemStarted != null)
			{
				this.UseItemStarted(item, msg);
			}
			if (this.UseItemSucceed != null)
			{
				this.UseItemSucceed(item);
			}
		}).On<OK>(delegate
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
			if (this.UseItemSucceed != null)
			{
				this.UseItemSucceed(item);
			}
		});
	}

	private void ReceiveAskEatFoodOverrideStatusEffectMsg(AskEatFoodOverrideStatusEffect msg, PacketHeader header)
	{
		UIManager.MessageBox.Show(T._("이전 음식의 효과가 사라집니다.\n계속 하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				ItemData itemData = FindItem(msg.ItemId);
				if (itemData != null)
				{
					UseItem(itemData, playerAccepted: true);
				}
			}
		});
	}

	public static DumpItems MakeDumpItemsPacket(Durango.Logic.Item.Inventory inven, string[] itemIds)
	{
		DumpItems result = default(DumpItems);
		switch (inven.Type)
		{
		case Durango.Logic.Item.Inventory.InventoryType.Artifact:
			result.SourceProp = new PropKey
			{
				EntityId = inven.OwnerId,
				Tile = inven.OwnerPosition
			};
			break;
		case Durango.Logic.Item.Inventory.InventoryType.Pet:
			result.SourcePetEntityId = inven.OwnerId;
			break;
		case Durango.Logic.Item.Inventory.InventoryType.Warehouse:
			result.SourceProp = new PropKey
			{
				EntityId = inven.OwnerId,
				Tile = inven.OwnerPosition
			};
			result.SectionName = inven.SelectedCategory;
			break;
		}
		result.ItemIds = itemIds;
		return result;
	}

	public static void DropItems(DumpItems dumpItems)
	{
		if (dumpItems.Tile.HasValue)
		{
			Vector3 pos = Durango.Terrain.Util.TilePositionToClientPosition(dumpItems.Tile.Value, tileCenter: true);
			Durango.Utils.Singleton<PlayerController>.Instance().MoveToPosition(pos, delegate
			{
				Connections.Frontend.Send(dumpItems);
			});
		}
		else
		{
			Connections.Frontend.Send(dumpItems);
		}
	}

	public void LockItem(bool locked, string[] itemIds)
	{
		if (KUtility.GetSize(itemIds) != 0)
		{
			Connections.Frontend.Send(new LockOrUnlockItems
			{
				Lock = locked,
				ItemIds = itemIds
			});
		}
	}

	public static void PutInItems(string target, Point2 tile, string[] items)
	{
		if (items.Length != 0)
		{
			Connections.Frontend.Send(new PutInItem
			{
				EntityId = target,
				Tile = tile,
				ItemIds = items
			});
		}
	}

	public static void PutInItemsIntoPet(string petId, string[] items)
	{
		if (items.Length != 0)
		{
			Connections.Frontend.Send(new PutInItemsIntoPet
			{
				PetId = petId,
				ItemIds = items
			});
		}
	}

	public static void PutInItemsIntoWarehouse(string target, Point2 tile, string sectionId, string[] items)
	{
		if (items.Length != 0)
		{
			Connections.Frontend.Send(new AddItemsToWarehouse
			{
				EntityId = target,
				Tile = tile,
				SectionName = sectionId,
				ItemIds = items
			});
		}
	}

	public static void TakeOutItems(string target, Point2 tile, string[] items, Action<bool> onResult = null)
	{
		ReplyMessageHandlerRegistrar replyMessageHandlerRegistrar = Connections.Frontend.Send(new TakeOutItem
		{
			EntityId = target,
			Tile = tile,
			ItemIds = items
		});
		if (onResult != null)
		{
			replyMessageHandlerRegistrar.All(delegate(Packet packet)
			{
				onResult(Packet.IsSuccess(packet));
			});
		}
	}

	public static void TakeOutItemsFromPet(string petId, string[] items)
	{
		Connections.Frontend.Send(new TakeOutItemsFromPet
		{
			PetId = petId,
			ItemIds = items
		});
	}

	public static void TakeOutItemsFromWarehouse(string target, Point2 tile, string sectionId, string[] items)
	{
		Connections.Frontend.Send(new PopItemsFromWarehouse
		{
			EntityId = target,
			Tile = tile,
			SectionName = sectionId,
			ItemIds = items
		});
	}

	public static void MoveToItemsFromWarehouse(string target, Point2 tile, string from, string to, string[] items)
	{
		Connections.Frontend.Send(new MoveItemsInWarehouse
		{
			EntityId = target,
			Tile = tile,
			SourceSectionName = from,
			TargetSectionName = to,
			ItemIds = items
		});
	}

	public static void ReceiveCargoImmediately(ReceiveCargoImmediately msg, Action<bool> onResult)
	{
		Connections.Frontend.Send(msg).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(Packet.IsSuccess(packet));
			}
		});
	}

	public ItemData FindItem(string itemid)
	{
		return _playerInventory.Find(itemid);
	}

	public void UpdateEquipments(EquipSlotType current)
	{
		int count = _playerInventory.Items.Count;
		for (int i = 0; i < count; i++)
		{
			_playerInventory.Items[i].IsEquipments = false;
		}
		UpdatePresetEquipments(current);
		UpdatePresetEquipments(EquipSlotType.Avatar);
		if (this.PlayerInventoryUpdated != null)
		{
			this.PlayerInventoryUpdated();
		}
	}

	private void UpdatePresetEquipments(EquipSlotType presetType)
	{
		EquipSystem.EquipPreset equipPreset = GameSystem<EquipSystem>.Instance().GetEquipPreset(presetType);
		if (equipPreset == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> slotItem in equipPreset.SlotItems)
		{
			ItemData itemData = FindItem(slotItem.Value);
			if (itemData != null)
			{
				itemData.IsEquipments = true;
			}
		}
	}

	public int GetFilteredItemCount(SingularTagFilter[] filters, bool allowLocked = false)
	{
		int num = 0;
		int count = _playerInventory.Items.Count;
		for (int i = 0; i < count; i++)
		{
			ItemData itemData = _playerInventory.Items[i];
			if ((allowLocked || !itemData.Locked) && itemData.HasTag(filters[i].Id))
			{
				num++;
			}
		}
		return num;
	}

	public int GetTaggedItemCount(IItemEvaluator evaluator, bool allowLocked = false)
	{
		if (evaluator == null)
		{
			return 0;
		}
		int num = 0;
		int count = _playerInventory.Items.Count;
		for (int i = 0; i < count; i++)
		{
			ItemData itemData = _playerInventory.Items[i];
			if ((allowLocked || !itemData.Locked) && evaluator.Evaluate(itemData))
			{
				num++;
			}
		}
		return num;
	}

	public int GetTaggedItemCount(OrTagFilter tags, OrTagFilter materials, bool allowEquipped = false)
	{
		if (tags == null || tags.Count == 0)
		{
			return 0;
		}
		return Durango.Logic.Item.Util.Counting(_playerInventory.Items, (ItemData item) => item.HasTagsAndMaterials(tags, materials) && (!item.IsEquipments || allowEquipped));
	}

	public List<ItemData> FilteringByTag(string tagId)
	{
		List<ItemData> list = new List<ItemData>();
		int count = _playerInventory.Items.Count;
		for (int i = 0; i < count; i++)
		{
			ItemData itemData = _playerInventory.Items[i];
			if (itemData.HasTag(tagId))
			{
				list.Add(itemData);
			}
		}
		return list;
	}

	[CanBeNull]
	public static ItemData GetOrMakeItemData(Item item)
	{
		if (string.IsNullOrEmpty(item.Id))
		{
			return null;
		}
		ItemData itemData = ((!GameSystem<InventorySystem>.HasInstance()) ? null : GameSystem<InventorySystem>.Instance().FindItem(item.Id));
		if (itemData == null)
		{
			return new ItemData(item);
		}
		return itemData;
	}

	public static void ChangeDisplay(ChangePlayerDisplay display, Action<bool> onResult)
	{
		Connections.Frontend.Send(display).All(delegate(Packet packet)
		{
			bool obj = Packet.IsSuccess(packet);
			if (onResult != null)
			{
				onResult(obj);
			}
		});
	}
}

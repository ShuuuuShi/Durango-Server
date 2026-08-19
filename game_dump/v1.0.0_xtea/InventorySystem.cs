using System;
using System.Collections.Generic;
using System.Linq;
using ItemSystem;
using K1Network;
using L10N;
using Messages;
using Shared.Economy;
using Shared.System;
using StatusEffectData;
using TimerData;

public class InventorySystem : GameSystem<InventorySystem>
{
	private bool _initInventoryList;

	private readonly ItemSystem.Inventory _playerInventory = new ItemSystem.Inventory();

	private readonly ItemSystem.Inventory _trakingInventory = new ItemSystem.Inventory();

	public ItemSystem.Inventory PlayerInventory => _playerInventory;

	public List<ItemData> PlayerItemList => _playerInventory.Items;

	public ItemSystem.Inventory TrakingInventory => _trakingInventory;

	public event Action<ItemData> OnCollectItem;

	public event Action<ItemData, Collected> OnCollected;

	public event Action PlayerInventoryUpdated;

	public event Action PlayerBalanceUpdated;

	public event Action<ulong> PlayerItemExpired;

	public event Action TrakingInventoryUpdated;

	public event Action<ItemData> OnUseItemSucceed;

	private void Awake()
	{
		Connections.Frontend.On<Messages.Inventory>(ReceiveInventoryMsg);
		Connections.Frontend.On<ItemUsed>(ReceiveItemUsedMsg);
		Connections.Frontend.On<ItemExpired>(ReceiveItemExpiredMsg);
		Connections.Frontend.On<Messages.Warehouse>(ReceiveWarehouseMsg);
		Connections.Frontend.On<InventoryUpdated>(ReceiveInventoryUpdatedMsg);
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			Connections.Frontend.Send(default(GetEquipments));
			Connections.Frontend.Send(default(GetInventory));
		};
		KSingleton<GameManager>.Instance().PreReconnect += delegate
		{
			PlayerInventory.ClearSequence();
		};
	}

	private void Start()
	{
		GameSystem<ItemCraftingSystem>.Instance().CraftingFinished += OnCraftFinished;
	}

	private void ReceiveInventoryMsg(Messages.Inventory msg, PacketHeader header)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			UpdatePlayerInventory(msg);
		}
		else if (_trakingInventory.OwnerId == msg.EntityId)
		{
			UpdateTrakingInventory(msg);
		}
	}

	private void ReceiveItemExpiredMsg(ItemExpired msg, PacketHeader header)
	{
		if (this.PlayerItemExpired != null)
		{
			this.PlayerItemExpired(msg.ItemId);
		}
		UIManager.SystemMsg(msg.Text, 4f);
	}

	private void ReceiveInventoryUpdatedMsg(InventoryUpdated msg, PacketHeader header)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			UpdatePlayerBalance(msg.Balance);
			if (_playerInventory.RecentSequence < msg.Seq)
			{
				_playerInventory.RecentSequence = msg.Seq;
				if (this.PlayerInventoryUpdated != null)
				{
					_playerInventory.UpdateIfNeeded();
				}
			}
			if (this.PlayerBalanceUpdated != null)
			{
				this.PlayerBalanceUpdated();
			}
		}
		if (msg.EntityId == _trakingInventory.OwnerId && msg.Tile.HasValue)
		{
			PropKey propKey = default(PropKey);
			propKey.EntityId = msg.EntityId;
			propKey.Tile = msg.Tile.Value;
			PropKey value = propKey;
			Connections.Frontend.Send(new GetInventory
			{
				Target = value
			});
		}
	}

	private void ReceiveWarehouseMsg(Messages.Warehouse msg, PacketHeader header)
	{
		if (msg.EntityId == _trakingInventory.OwnerId)
		{
			UpdateTrakingInventory(msg);
		}
	}

	public void UpdatePlayerInventory(Messages.Inventory msg)
	{
		List<ItemData> items = _playerInventory.Items;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			items[i].Valid = false;
		}
		ulong[] array = msg.LabeledItemIds.Get(1);
		int j = 0;
		for (int num = msg.Items.Length; j < num; j++)
		{
			int num2 = -1;
			Item itemInfo = msg.Items[j];
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
				if (_initInventoryList)
				{
					itemData.NewChecker.IsNew = true;
				}
			}
			else
			{
				itemData = items[num2];
				itemData.Set(itemInfo);
			}
			if (!_initInventoryList)
			{
				NewCheckUtil.Refresh(itemData);
			}
			itemData.Valid = true;
			itemData.IsEquipments = GameSystem<EquipSystem>.Instance().IsEquipItem(itemData) != null;
			itemData.Like = array != null && Array.IndexOf(array, itemData.Id) != -1;
		}
		for (int num3 = items.Count - 1; num3 >= 0; num3--)
		{
			if (!items[num3].Valid)
			{
				items.RemoveAt(num3);
			}
		}
		_playerInventory.Capacity = msg.MaxSize;
		UpdatePlayerBalance(msg.Balance);
		if (this.PlayerBalanceUpdated != null)
		{
			this.PlayerBalanceUpdated();
		}
		_playerInventory.Sequence = msg.Seq;
		_initInventoryList = true;
		if (this.PlayerInventoryUpdated != null)
		{
			this.PlayerInventoryUpdated();
		}
		ItemData itemData2 = FindItem(_trakingInventory.OwnerId);
		if (itemData2 != null && itemData2.Reins != null)
		{
			UpdateTrakingInventory(itemData2.Reins.Contents, itemData2.Reins.Capacity);
		}
	}

	private void UpdatePlayerBalance(Dictionary<Currency, long> balance)
	{
		Dictionary<Currency, long> balance2 = _playerInventory.Balance;
		_playerInventory.Balance = balance;
		if (balance2 != null)
		{
			long num = balance2.Get(Currency.TStone, 0L);
			long num2 = balance.Get(Currency.TStone, 0L);
			if (num2 > num)
			{
				UIManager.IndicatorMsg(ItemSystem.Inventory.CurrencyFormat(num2 - num, Currency.TStone));
			}
		}
	}

	private void UpdateTrakingInventory(Messages.Inventory msg)
	{
		_trakingInventory.OwnerId = msg.EntityId;
		List<ItemData> items = _trakingInventory.Items;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			items[i].Valid = false;
		}
		int j = 0;
		for (int num = msg.Items.Length; j < num; j++)
		{
			int num2 = -1;
			Item itemInfo = msg.Items[j];
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
		_trakingInventory.Capacity = msg.MaxSize;
		_trakingInventory.Balance = msg.Balance;
		if (this.TrakingInventoryUpdated != null)
		{
			this.TrakingInventoryUpdated();
		}
	}

	private void UpdateTrakingInventory(IList<ItemData> list, float capacity, Dictionary<Currency, long> balance = null)
	{
		List<ItemData> items = _trakingInventory.Items;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			items[i].Valid = false;
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			int num = -1;
			ItemData itemData = list[j];
			int k = 0;
			for (int count3 = items.Count; k < count3; k++)
			{
				if (items[k].Id == itemData.Id)
				{
					num = k;
					break;
				}
			}
			if (num == -1)
			{
				items.Add(itemData);
			}
			else
			{
				items[num] = itemData;
			}
			itemData.Valid = true;
		}
		for (int num2 = items.Count - 1; num2 >= 0; num2--)
		{
			if (!items[num2].Valid)
			{
				items.RemoveAt(num2);
			}
		}
		_trakingInventory.Capacity = capacity;
		_trakingInventory.Balance = balance;
		if (this.TrakingInventoryUpdated != null)
		{
			this.TrakingInventoryUpdated();
		}
	}

	private void UpdateTrakingInventory(Messages.Warehouse warehouse)
	{
		_trakingInventory.Items.Clear();
		_trakingInventory.Capacity = 200f;
		_trakingInventory.Balance = null;
		_trakingInventory.Categories = warehouse.CategorySizes;
		_trakingInventory.CategoryCapacity = warehouse.CategoryCount;
		_trakingInventory.SelectedCategory = null;
		if (this.TrakingInventoryUpdated != null)
		{
			this.TrakingInventoryUpdated();
		}
	}

	public void GetWarehouseCategory(int index)
	{
		if (_trakingInventory.Type != ItemSystem.Inventory.InventoryType.Warehouse)
		{
			return;
		}
		int size = KUtility.GetSize(_trakingInventory.Categories);
		if (index < 0 || index >= size)
		{
			return;
		}
		_trakingInventory.Items.Clear();
		string cat = _trakingInventory.Categories[index].Key;
		_trakingInventory.SelectedCategory = cat;
		Connections.Frontend.Send(new GetCategorizedItems
		{
			EntityId = _trakingInventory.OwnerId,
			Tile = _trakingInventory.OwnerPosition,
			Category = cat
		}).On(delegate(CategorizedItems msg, PacketHeader header)
		{
			if (!(_trakingInventory.SelectedCategory != cat))
			{
				int i = 0;
				for (int size2 = KUtility.GetSize(msg.Items); i < size2; i++)
				{
					_trakingInventory.Items.Add(new ItemData(msg.Items[i]));
				}
				_trakingInventory.Balance = null;
				if (this.TrakingInventoryUpdated != null)
				{
					this.TrakingInventoryUpdated();
				}
			}
		});
	}

	public void SetResurrectionReward(IList<ItemData> rewards)
	{
		ulong[] itemIds = rewards.Select((ItemData t) => t.Id).ToArray();
		Connections.Frontend.Send(new SetResurrectionRewards
		{
			ItemIds = itemIds
		});
	}

	private void OnCraftFinished(IList<ItemData> items, string recipeId)
	{
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			ItemData itemData = FindItem(items[i].Id);
			if (itemData != null)
			{
				itemData.NewChecker.IsNew = true;
			}
		}
	}

	public void DrawWater(ulong toolId)
	{
		Connections.Frontend.Send(new DrawWater
		{
			ToolItemId = toolId
		}).On(delegate(Messages.Timer msg, PacketHeader _)
		{
			TimerSystem.SetGaugeAndPlayMotion(msg.Duration, IconMap.Get(Interaction.DrawWater), "Water_Gain");
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.DrawWater, T._("강물"));
		});
	}

	public void SortItemList(Util.SortOption option, ItemSystem.Inventory inventory = null, bool descending = true)
	{
		if (inventory == null)
		{
			inventory = _playerInventory;
		}
		Util.SortItems(inventory.Items, option, descending);
	}

	public void CollectedReceived(Collected m)
	{
		ItemData itemData = ((m.Item.Id != 0L) ? new ItemData(m.Item) : null);
		if (this.OnCollected != null)
		{
			this.OnCollected(itemData, m);
		}
		if (itemData != null && this.OnCollectItem != null)
		{
			this.OnCollectItem(itemData);
		}
	}

	public void SetArtifactInventory(ulong target, Point2 tile)
	{
		_trakingInventory.OwnerId = target;
		_trakingInventory.OwnerPosition = tile;
		_trakingInventory.Type = ItemSystem.Inventory.InventoryType.Artifact;
		_trakingInventory.Items.Clear();
		_trakingInventory.Balance = null;
	}

	public void SetWarehouseInventory(ulong target, Point2 tile)
	{
		SetArtifactInventory(target, tile);
		_trakingInventory.Type = ItemSystem.Inventory.InventoryType.Warehouse;
		Connections.Frontend.Send(new GetWarehouse
		{
			EntityId = target,
			Tile = tile
		});
	}

	public void SetReinsInventory(ulong id)
	{
		ItemData itemData = FindItem(id);
		if (itemData != null && itemData.Reins != null)
		{
			_trakingInventory.OwnerId = id;
			_trakingInventory.OwnerPosition = -Point2.one;
			_trakingInventory.Type = ItemSystem.Inventory.InventoryType.Rein;
			UpdateTrakingInventory(itemData.Reins.Contents, itemData.Reins.Capacity);
		}
	}

	public void ResetTrakingInventory()
	{
		_trakingInventory.OwnerId = 0uL;
		_trakingInventory.OwnerPosition = null;
		_trakingInventory.Type = ItemSystem.Inventory.InventoryType.Invalid;
		_trakingInventory.Items.Clear();
		_trakingInventory.Balance = null;
		_trakingInventory.Categories = null;
		_trakingInventory.CategoryCapacity = 0;
		_trakingInventory.SelectedCategory = null;
	}

	private void ReceiveItemUsedMsg(ItemUsed m, PacketHeader header)
	{
		if (!string.IsNullOrEmpty(m.Motion))
		{
			KSingleton<PlayerController>.Instance().Motion(m.Motion, m.Time);
		}
		if (!string.IsNullOrEmpty(m.Msg))
		{
			DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Eat, m.Msg);
			UIManager.SystemMsg(m.Msg, 4f);
		}
	}

	public void ChangeWarehouseCategoryName(string oldName, string newName)
	{
		if (_trakingInventory.Type == ItemSystem.Inventory.InventoryType.Warehouse)
		{
			Connections.Frontend.Send(new RenameCategory
			{
				EntityId = _trakingInventory.OwnerId,
				Tile = _trakingInventory.OwnerPosition,
				Category = oldName,
				NewCategory = newName
			});
		}
	}

	public void SetWarehouseCategoryList(string[] list)
	{
		if (_trakingInventory.Type == ItemSystem.Inventory.InventoryType.Warehouse)
		{
			Connections.Frontend.Send(new SetCategoryOrder
			{
				EntityId = _trakingInventory.OwnerId,
				Tile = _trakingInventory.OwnerPosition,
				CategoryOrder = list
			});
		}
	}

	public void AddWarehouseCategory(string key)
	{
		if (_trakingInventory.Type == ItemSystem.Inventory.InventoryType.Warehouse)
		{
			Connections.Frontend.Send(new MakeCategory
			{
				EntityId = _trakingInventory.OwnerId,
				Tile = _trakingInventory.OwnerPosition,
				Category = key
			});
		}
	}

	public void RemoveWarehouseCategory(string key)
	{
		if (_trakingInventory.Type != ItemSystem.Inventory.InventoryType.Warehouse)
		{
			return;
		}
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_trakingInventory.Categories); i < size; i++)
		{
			if (_trakingInventory.Categories[i].Key == key)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			UIManager.SystemMsg(T._("{0:을} 찾을 수 없습니다"), key);
			return;
		}
		if (_trakingInventory.Categories[num].Value > 0)
		{
			UIManager.SystemMsg(T._("{0:이} 비어있지 않습니다", key));
			return;
		}
		Connections.Frontend.Send(new RemoveCategory
		{
			EntityId = _trakingInventory.OwnerId,
			Tile = _trakingInventory.OwnerPosition,
			Category = key
		});
	}

	public void UpdateItemListOrder()
	{
		SendItemLocationInfo(_playerInventory);
		if (_trakingInventory.Type == ItemSystem.Inventory.InventoryType.Artifact)
		{
			SendItemLocationInfo(_trakingInventory);
		}
	}

	private void SendItemLocationInfo(ItemSystem.Inventory inventory)
	{
		if (inventory != null && inventory.Items != null)
		{
			List<ItemData> items = inventory.Items;
			int count = items.Count;
			InventoryOrder msg = default(InventoryOrder);
			if (inventory.Type == ItemSystem.Inventory.InventoryType.Artifact)
			{
				msg.TargetArtifact = new PropKey
				{
					EntityId = inventory.OwnerId,
					Tile = inventory.OwnerPosition
				};
			}
			else
			{
				msg.TargetArtifact = null;
			}
			msg.ItemOrder = new ulong[count];
			for (int i = 0; i < count; i++)
			{
				msg.ItemOrder[i] = items[i].Id;
			}
			Connections.Frontend.Send(msg);
		}
	}

	public void UseItem(ItemData item)
	{
		bool flag = false;
		StatusEffectData.StatusEffect statusEffect = GameSystem<PlayerStatusEffectSystem>.Instance().GetStatusEffect("satiety_high");
		if (statusEffect == null && (item.HasTag("eatable") || item.HasTag("drinkable")))
		{
			StatusEffectData.StatusEffect statusEffect2 = GameSystem<PlayerStatusEffectSystem>.Instance().GetStatusEffect("food_power");
			if (statusEffect2 != null)
			{
				PerformanceData performanceData = item.GetPerformanceData("food");
				float value = 0f;
				if (performanceData != null && performanceData.num_attrs.TryGetValue("modifier_effect_time", out value) && value > 0f)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			UIManager.MessageBox.Show(T._("이전 음식의 효과가 사라집니다.\n계속 하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					SendUseItem(item);
				}
			});
		}
		else
		{
			SendUseItem(item);
		}
	}

	private void SendUseItem(ItemData item)
	{
		Connections.Frontend.Send(new UseItem
		{
			ItemId = item.Id
		}).On(delegate(StartTimer msg, PacketHeader header)
		{
			TimerData.Timer timer = new TimerData.Timer(msg.EntityId, msg.Subject, msg.Time + msg.AdditionalTime, msg.Current);
			IconProgressGauge iconProgressGauge = TimerData.Timer.Play<IconProgressGauge>(timer);
			iconProgressGauge.SetIcon(item.Icon);
			if (this.OnUseItemSucceed != null)
			{
				this.OnUseItemSucceed(item);
			}
		}).On<OK>(delegate
		{
			if (this.OnUseItemSucceed != null)
			{
				this.OnUseItemSucceed(item);
			}
		});
	}

	public void DropItems(params ItemData[] item)
	{
		int num = item.Length;
		DumpItems msg = default(DumpItems);
		msg.ItemIds = new ulong[num];
		for (int i = 0; i < num; i++)
		{
			msg.ItemIds[i] = item[i].Id;
		}
		Connections.Frontend.Send(msg);
	}

	public void LikeItem(params ItemData[] items)
	{
		bool active = false;
		ulong[] array = new ulong[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			array[i] = items[i].Id;
			if (!items[i].Like)
			{
				active = true;
			}
		}
		Connections.Frontend.Send(new LabelItems
		{
			Label = 1,
			Active = active,
			ItemIds = array
		});
	}

	public static void PutInItems(ulong target, Point2 tile, params ulong[] items)
	{
		Connections.Frontend.Send(new PutInItem
		{
			EntityId = target,
			Tile = tile,
			ItemIds = items
		});
	}

	public static void PutInItemsIntoPet(ulong petId, params ulong[] items)
	{
		Connections.Frontend.Send(new PutInItemsIntoPet
		{
			PetId = petId,
			ItemIds = items
		});
	}

	public static void PutInItemsIntoWarehouse(ulong target, Point2 tile, string category, params ulong[] items)
	{
		Connections.Frontend.Send(new AddItemsToWarehouse
		{
			EntityId = target,
			Tile = tile,
			Category = category,
			ItemIds = items
		});
	}

	public static void TakeOutItems(ulong target, Point2 tile, params ulong[] items)
	{
		Connections.Frontend.Send(new TakeOutItem
		{
			EntityId = target,
			Tile = tile,
			ItemIds = items
		});
	}

	public static void TakeOutItemsFromPet(ulong petId, params ulong[] items)
	{
		Connections.Frontend.Send(new TakeOutItemsFromPet
		{
			PetId = petId,
			ItemIds = items
		});
	}

	public static void TakeOutItemsFromWarehouse(ulong target, Point2 tile, string category, params ulong[] items)
	{
		Connections.Frontend.Send(new PopItemsFromWarehouse
		{
			EntityId = target,
			Tile = tile,
			Category = category,
			ItemIds = items
		});
	}

	public static void MoveToItemsFromWarehouse(ulong target, Point2 tile, string from, string to, params ulong[] items)
	{
		Connections.Frontend.Send(new MoveItemsInWarehouse
		{
			EntityId = target,
			Tile = tile,
			SourceCategory = from,
			TargetCategory = to,
			ItemIds = items
		});
	}

	public void FeedPet(ulong target, params ulong[] items)
	{
		Connections.Frontend.Send(new Feeding
		{
			PetId = target,
			FoodIds = items
		});
	}

	public ItemData FindItem(ulong itemid)
	{
		return _playerInventory.Find(itemid);
	}

	public int GetFilteredItemCount(TagFilter[] filters, bool exceptEquip = false)
	{
		int num = 0;
		int count = _playerInventory.Items.Count;
		for (int i = 0; i < count; i++)
		{
			ItemData itemData = _playerInventory.Items[i];
			if (itemData.HasTag(filters) && (!exceptEquip || !itemData.IsEquipments))
			{
				num++;
			}
		}
		return num;
	}

	public int GetItemCount(IItemEvaluator predicate)
	{
		if (predicate == null)
		{
			return 0;
		}
		int num = 0;
		int count = _playerInventory.Items.Count;
		for (int i = 0; i < count; i++)
		{
			ItemData data = _playerInventory.Items[i];
			if (predicate.Evaluate(data))
			{
				num++;
			}
		}
		return num;
	}

	public int GetTaggedItemCount(TagEvaluator tagEval)
	{
		if (tagEval == null)
		{
			return 0;
		}
		int num = 0;
		int count = _playerInventory.Items.Count;
		for (int i = 0; i < count; i++)
		{
			ItemData data = _playerInventory.Items[i];
			if (tagEval.Evaluate(data))
			{
				num++;
			}
		}
		return num;
	}

	public int GetTaggedItemCount(IList<TagFilter> tags, IList<TagFilter> materials, bool allowEquipped = false)
	{
		if (tags == null || tags.Count == 0)
		{
			return 0;
		}
		return Util.Counting(_playerInventory.Items, (ItemData item) => item.HasTagsAndMaterials(tags, materials) && (!item.IsEquipments || allowEquipped));
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
}

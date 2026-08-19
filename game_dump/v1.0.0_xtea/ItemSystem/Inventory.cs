using System;
using System.Collections.Generic;
using K1Network;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Item;

namespace ItemSystem;

public class Inventory
{
	public enum InventoryType
	{
		Invalid,
		Artifact,
		Rein,
		Warehouse
	}

	public enum InventoryMode
	{
		Invaild = -1,
		Normal,
		Exchange,
		Dead
	}

	public const int CategoryMax = 200;

	public ulong OwnerId;

	public Point2 OwnerPosition;

	public InventoryType Type;

	public readonly List<ItemData> Items = new List<ItemData>();

	public float Capacity;

	public Dictionary<Currency, long> Balance;

	public KeyValuePair<string, int>[] Categories;

	public int CategoryCapacity;

	public string SelectedCategory;

	public uint Sequence;

	public uint RecentSequence;

	private uint? requestedSequence;

	private static List<KeyValuePair<string, UseType>> _itemUseTable = new List<KeyValuePair<string, UseType>>
	{
		new KeyValuePair<string, UseType>("eatable", UseType.Eat),
		new KeyValuePair<string, UseType>("drinkable", UseType.Drink),
		new KeyValuePair<string, UseType>("reins", UseType.ToggleSpawn)
	};

	private event Action updatedCallback;

	public void ClearSequence()
	{
		Sequence = 0u;
		RecentSequence = 0u;
		requestedSequence = null;
		this.updatedCallback = null;
	}

	public void UpdateIfNeeded(Action callback = null)
	{
		if (Sequence != 0 && Sequence == RecentSequence)
		{
			callback?.Invoke();
			return;
		}
		if (!requestedSequence.HasValue || requestedSequence.Value < RecentSequence)
		{
			requestedSequence = RecentSequence;
			Connections.Frontend.Send(default(GetInventory)).On(delegate(Messages.Inventory msg, PacketHeader header)
			{
				GameSystem<InventorySystem>.Instance().UpdatePlayerInventory(msg);
				if (this.updatedCallback != null && Sequence >= requestedSequence.Value)
				{
					this.updatedCallback();
					this.updatedCallback = null;
				}
			});
		}
		if (callback != null)
		{
			this.updatedCallback = (Action)Delegate.Combine(this.updatedCallback, callback);
		}
	}

	public long GetBalance(Currency currency)
	{
		return (Balance != null) ? Balance.Get(currency, 0L) : 0;
	}

	public int CurrentSize()
	{
		int num = 0;
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			num += Items[i].Size;
		}
		return num;
	}

	public ItemData Find(ulong id)
	{
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			if (Items[i].Id == id)
			{
				return Items[i];
			}
		}
		return null;
	}

	public void ShowPayConfirm(int amount, Currency currency, Gettext msg, Action<bool> action, params string[] param)
	{
		long balance = GetBalance(currency);
		string[] array = new string[param.Length + 2];
		array[0] = CurrencyFormat(amount, currency);
		array[1] = CurrencyFormat(balance, currency);
		param.CopyTo(array, 2);
		string comment = T.Format(msg, array);
		if (balance < amount)
		{
			UIManager.MessageBox.Show(comment, delegate
			{
				action(obj: false);
			}, T._("취소"));
		}
		else
		{
			UIManager.MessageBox.Show(comment, action);
		}
	}

	public static string CurrencyFormat(long amount, Currency currency)
	{
		return string.Format(currency switch
		{
			Currency.TStone => "<t_stone></t_stone>{0:N0}", 
			Currency.Gem => "<gem></gem>{0:N0}", 
			_ => "{0:N0}", 
		}, amount);
	}

	public static string GetIcon(Currency type)
	{
		return type switch
		{
			Currency.TStone => "tstone_icon", 
			Currency.Gem => "cashgem_icon", 
			_ => null, 
		};
	}

	public static void GetUsableActions(Inventory inventory, ItemData item, InventoryMode mode, ref Dictionary<UseType, int> usableSet)
	{
		if (item == null)
		{
			return;
		}
		switch (mode)
		{
		case InventoryMode.Dead:
			usableSet[UseType.Resurrection_Rewards] = usableSet.Get(UseType.Resurrection_Rewards, 0) + 1;
			return;
		case InventoryMode.Exchange:
			if (inventory == GameSystem<InventorySystem>.Instance().PlayerInventory)
			{
				usableSet[UseType.PutIn] = usableSet.Get(UseType.PutIn, 0) + 1;
			}
			else
			{
				usableSet[UseType.TakeOut] = usableSet.Get(UseType.TakeOut, 0) + 1;
			}
			return;
		}
		for (int i = 0; i < _itemUseTable.Count; i++)
		{
			KeyValuePair<string, UseType> keyValuePair = _itemUseTable[i];
			if (item.HasTag(keyValuePair.Key))
			{
				usableSet[keyValuePair.Value] = usableSet.Get(keyValuePair.Value, 0) + 1;
			}
		}
		if (item.ArtifactPackage != null)
		{
			switch (item.ArtifactPackage.Status)
			{
			case PackageStatus.Packing:
				usableSet[UseType.PackArtifact] = usableSet.Get(UseType.PackArtifact, 0) + 1;
				break;
			case PackageStatus.Sealed:
			case PackageStatus.Unpacking:
				usableSet[UseType.UnpackArtifact] = usableSet.Get(UseType.UnpackArtifact, 0) + 1;
				break;
			}
		}
		if (item.HasAttribute("slot"))
		{
			if (item.IsEquipments)
			{
				usableSet[UseType.UnEquip] = usableSet.Get(UseType.UnEquip, 0) + 1;
			}
			else
			{
				usableSet[UseType.Equip] = usableSet.Get(UseType.Equip, 0) + 1;
			}
		}
		if (item.Capsule != null)
		{
			usableSet[UseType.Place] = usableSet.Get(UseType.Place, 0) + 1;
		}
		if (Debug.isDebugBuild)
		{
			usableSet[UseType.CheatCopy] = usableSet.Get(UseType.CheatCopy, 0) + 1;
		}
	}

	public static bool CheckDisableUseType(IList<ItemIcon2> selectedItems, UseType useType)
	{
		switch (useType)
		{
		case UseType.PutIn:
		{
			Inventory trakingInventory = GameSystem<InventorySystem>.Instance().TrakingInventory;
			if (trakingInventory.Type == InventoryType.Warehouse)
			{
				break;
			}
			int num2 = 0;
			int j = 0;
			for (int count2 = selectedItems.Count; j < count2; j++)
			{
				ItemData item = selectedItems[j].Item;
				num2 += item.Size;
			}
			return trakingInventory.Capacity - (float)trakingInventory.CurrentSize() < (float)num2;
		}
		case UseType.TakeOut:
		{
			int num = 0;
			int i = 0;
			for (int count = selectedItems.Count; i < count; i++)
			{
				num += selectedItems[i].Item.Size;
			}
			Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
			return playerInventory.Capacity - (float)playerInventory.CurrentSize() < (float)num;
		}
		case UseType.ToggleSpawn:
		case UseType.Place:
			return selectedItems.Count > 1;
		}
		return false;
	}
}

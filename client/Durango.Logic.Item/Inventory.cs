using System.Collections.Generic;
using Durango.Network;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Item;

public class Inventory
{
	public enum InventoryMode
	{
		Normal,
		Exchange,
		Dead
	}

	public enum InventoryType
	{
		Player,
		Artifact,
		Pet,
		Warehouse
	}

	public enum InventoryState
	{
		Invalid,
		Loading,
		Loaded
	}

	public const string ClanFundIcon = "tstone_icon_clan";

	public string OwnerId;

	public Point2 OwnerPosition;

	public bool OnlyTakeOut;

	public InventoryType Type;

	[NotNull]
	public readonly List<ItemData> Items = new List<ItemData>();

	public int Capacity;

	public string[] StorableTags;

	public string[] UnstorableTags;

	[NotNull]
	public List<KeyValuePair<string, int>> Categories = new List<KeyValuePair<string, int>>();

	public int CategoryCapacity;

	public string SelectedCategory;

	public InventoryState State { get; private set; }

	public void Reset()
	{
		OwnerId = string.Empty;
		OwnerPosition = Point2.zero;
		OnlyTakeOut = false;
		Items.Clear();
		Capacity = 0;
		StorableTags = null;
		UnstorableTags = null;
		Categories.Clear();
		CategoryCapacity = 0;
		SelectedCategory = null;
		State = InventoryState.Invalid;
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

	public ItemData Find(string id)
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

	public bool CanPutIn(IList<ItemData> items)
	{
		if (items.Count > 1)
		{
			int num = 0;
			int i = 0;
			for (int count = items.Count; i < count; i++)
			{
				num += items[i].Size;
			}
			return Capacity >= num + CurrentSize();
		}
		return Capacity > CurrentSize();
	}

	public void SetOrder(string[] itemOrder)
	{
		if (itemOrder == null || itemOrder.Length == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < itemOrder.Length; i++)
		{
			int num2 = -1;
			string text = itemOrder[i];
			for (int j = num; j < Items.Count; j++)
			{
				if (Items[j].Id == text)
				{
					num2 = j;
					break;
				}
			}
			if (num2 != -1)
			{
				ItemData value = Items[num];
				Items[num] = Items[num2];
				Items[num2] = value;
				num++;
			}
		}
	}

	public void Request()
	{
		if (State == InventoryState.Invalid)
		{
			switch (Type)
			{
			case InventoryType.Player:
				return;
			case InventoryType.Artifact:
				Connections.Frontend.Send(new GetInventory
				{
					Target = new PropKey
					{
						EntityId = OwnerId,
						Tile = OwnerPosition
					}
				});
				break;
			case InventoryType.Pet:
				Connections.Frontend.Send(new GetPetInventory
				{
					EntityId = OwnerId
				});
				break;
			case InventoryType.Warehouse:
				Connections.Frontend.Send(new GetWarehouse
				{
					EntityId = OwnerId,
					Tile = OwnerPosition
				});
				break;
			}
			State = InventoryState.Loading;
		}
	}

	public void Requested()
	{
		State = InventoryState.Loaded;
	}

	public static string CurrencyFormat(long amount)
	{
		return amount.ToString("N0", T.Culture);
	}

	public static string CurrencyFormat(long amount, Currency currency)
	{
		return string.Format(format: currency.Normalize() switch
		{
			Currency.TStone => "<t_stone/> {0:N0}", 
			Currency.Gem => "<gem/> {0:N0}", 
			Currency.MobileCoin => "<mobile_coin/> {0:N0}", 
			Currency.PcCoin => "<pc_coin/> {0:N0}", 
			Currency.CashshopMileage => "<cashshop_mileage/> {0:N0}", 
			Currency.RPiece => "<r_piece/> {0:N0}", 
			Currency.WarpMatter => "<warp_matter/> {0:N0}", 
			_ => "{0:N0}", 
		}, provider: T.Culture, args: new object[1] { amount });
	}

	public static string CurrencyEmphasisFormat(long amount, Currency currency)
	{
		if (amount <= 0)
		{
			return T._("무료!");
		}
		return string.Format(format: currency.Normalize() switch
		{
			Currency.TStone => "[preset=round_box?<t_stone/>    {0:N0}]", 
			Currency.Gem => "[preset=round_box?<gem/>    {0:N0}]", 
			Currency.MobileCoin => "[preset=round_box?<mobile_coin/>    {0:N0}]", 
			Currency.PcCoin => "[preset=round_box?<pc_coin/>    {0:N0}]", 
			Currency.CashshopMileage => "[preset=round_box?<cashshop_mileage/>    {0:N0}]", 
			Currency.RPiece => "[preset=round_box?<r_piece/>    {0:N0}]", 
			Currency.WarpMatter => "[preset=round_box?<warp_matter/>    {0:N0}]", 
			_ => "<em>{0:N0}</em>", 
		}, provider: T.Culture, args: new object[1] { amount });
	}

	public static string ToCurrencyButtonText(string text, long amount, Currency currency)
	{
		return $"{text}  {CurrencyEmphasisFormat(amount, currency)}";
	}

	public static string ToVoucherButtonText(string text, int amount, string voucherId)
	{
		return $"{text}  {SingletonDict<string, Voucher>.Get(voucherId).GetEmphasisCostFormat(amount)}";
	}

	public static string CurrencyFormat(long amount, string icon, float iconScale = 1f)
	{
		if (iconScale == 1f)
		{
			return string.Format(T.Culture, "[c][ffffff][icon={1}][-][/c] {0:N0}", amount, icon);
		}
		return string.Format(T.Culture, "[c][ffffff][icon={1}:{2}][-][/c] {0:N0}", amount, icon, iconScale);
	}

	public static string GetIcon(Currency type)
	{
		return type.Normalize() switch
		{
			Currency.TStone => "tstone_icon", 
			Currency.Gem => "warpgem_icon", 
			Currency.MobileCoin => "durango_coin", 
			Currency.PcCoin => "durango_coin_pc", 
			Currency.CashshopMileage => "icon_mileage", 
			Currency.RPiece => "r_piece_icon", 
			Currency.WarpMatter => "warp_matter_icon", 
			_ => null, 
		};
	}

	public static bool CheckEnableUseType(IList<ItemData> selectedItems, UseType useType)
	{
		switch (useType)
		{
		case UseType.PutIn:
		{
			Inventory trackingInventory = GameSystem<InventorySystem>.Instance().TrackingInventory;
			if (trackingInventory.State == InventoryState.Invalid)
			{
				return true;
			}
			if (trackingInventory.OnlyTakeOut)
			{
				return false;
			}
			if (trackingInventory.Type == InventoryType.Warehouse)
			{
				return true;
			}
			return trackingInventory.CanPutIn(selectedItems);
		}
		case UseType.TakeOut:
		{
			Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
			return playerInventory.CanPutIn(selectedItems);
		}
		case UseType.ChangeDisplay:
		case UseType.Place:
		case UseType.Build:
		case UseType.Dye:
			return selectedItems.Count <= 1;
		case UseType.Taming:
			return false;
		case UseType.Imprint:
			if (selectedItems.Count == 1)
			{
				ItemData itemData = selectedItems[0];
				return itemData.CanImprint();
			}
			return false;
		default:
			return true;
		}
	}
}

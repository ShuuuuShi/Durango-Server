using MsgPack;
using Shared.Market;

namespace Messages;

public struct Product
{
	public ulong Id;

	public ulong RegionId;

	public ulong MarketId;

	public ulong SellerId;

	public ulong? BuyerId;

	public double ListedAt;

	public double ExpiresAt;

	public double? PurchasedAt;

	public Price Price;

	public Item[] Items;

	public ProductState State;

	public Point2 Tile;

	public static void Pack(Packer packer, Product val, bool hint = false)
	{
		packer.PackArrayHeader(12);
		packer.Pack(val.Id);
		packer.Pack(val.RegionId);
		packer.Pack(val.MarketId);
		packer.Pack(val.SellerId);
		if (!val.BuyerId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.BuyerId.Value);
		}
		packer.Pack(val.ListedAt);
		packer.Pack(val.ExpiresAt);
		if (!val.PurchasedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PurchasedAt.Value);
		}
		Price.Pack(packer, val.Price);
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int i = 0; i < val.Items.Length; i++)
			{
				Item.Pack(packer, val.Items[i]);
			}
		}
		packer.Pack((int)val.State);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static Product Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Product result = default(Product);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.RegionId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.MarketId = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.SellerId = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData5)).IsNil)
		{
			result.BuyerId = null;
		}
		else
		{
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData6)).AsUInt64();
			result.BuyerId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.ListedAt = ((MessagePackObject)(ref lastReadData7)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		result.ExpiresAt = ((MessagePackObject)(ref lastReadData8)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData9)).IsNil)
		{
			result.PurchasedAt = null;
		}
		else
		{
			MessagePackObject lastReadData10 = unpacker.LastReadData;
			double value2 = ((MessagePackObject)(ref lastReadData10)).AsDouble();
			result.PurchasedAt = value2;
		}
		unpacker.Read();
		result.Price = Price.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData11)).AsInt32();
		result.Items = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData12 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData12)).AsInt32();
		if (num2 < 0 || 7 < num2)
		{
			result.State = ProductState.Invalid;
		}
		else
		{
			result.State = (ProductState)num2;
		}
		unpacker.Read();
		ushort num3 = default(ushort);
		unpacker.ReadUInt16(ref num3);
		result.Tile.x = num3;
		unpacker.ReadUInt16(ref num3);
		result.Tile.y = num3;
		return result;
	}

	public override string ToString()
	{
		return $"<Product Id={Id} RegionId={RegionId} MarketId={MarketId} SellerId={SellerId} BuyerId={BuyerId} ListedAt={ListedAt} ExpiresAt={ExpiresAt} PurchasedAt={PurchasedAt} Price={Price} Items={Items} State={State} Tile={Tile}>";
	}
}

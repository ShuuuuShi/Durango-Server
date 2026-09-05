using MsgPack;

namespace Messages;

public struct CommodityInfo
{
	public string Id;

	public int? MaxPurchasableCount;

	public double? PeriodicPurchasableAt;

	public int? PeriodicPurchasableCount;

	public double? SpecialDealExpiresAt;

	public static void Pack(Packer packer, CommodityInfo val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (!val.MaxPurchasableCount.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.MaxPurchasableCount.Value);
		}
		if (!val.PeriodicPurchasableAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PeriodicPurchasableAt.Value);
		}
		if (!val.PeriodicPurchasableCount.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PeriodicPurchasableCount.Value);
		}
		if (!val.SpecialDealExpiresAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.SpecialDealExpiresAt.Value);
		}
	}

	public static CommodityInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CommodityInfo result = default(CommodityInfo);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.MaxPurchasableCount = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.MaxPurchasableCount = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PeriodicPurchasableAt = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.PeriodicPurchasableAt = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PeriodicPurchasableCount = null;
		}
		else
		{
			int value3 = unpacker.LastReadData.AsInt32();
			result.PeriodicPurchasableCount = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SpecialDealExpiresAt = null;
		}
		else
		{
			double value4 = unpacker.LastReadData.AsDouble();
			result.SpecialDealExpiresAt = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CommodityInfo Id={Id} MaxPurchasableCount={MaxPurchasableCount} PeriodicPurchasableAt={PeriodicPurchasableAt} PeriodicPurchasableCount={PeriodicPurchasableCount} SpecialDealExpiresAt={SpecialDealExpiresAt}>";
	}
}

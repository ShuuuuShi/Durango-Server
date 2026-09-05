using MsgPack;
using Shared.Economy;

namespace Messages;

public struct SpecialDeal
{
	public string CommodityId;

	public double ExpiresAt;

	public Currency PriceCurrency;

	public long PriceAmount;

	public long? OriginalPriceAmount;

	public static void Pack(Packer packer, SpecialDeal val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.CommodityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CommodityId);
		}
		packer.Pack(val.ExpiresAt);
		packer.Pack((int)val.PriceCurrency);
		packer.Pack(val.PriceAmount);
		if (!val.OriginalPriceAmount.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.OriginalPriceAmount.Value);
		}
	}

	public static SpecialDeal Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SpecialDeal result = default(SpecialDeal);
		result.CommodityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ExpiresAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 7 < num)
		{
			result.PriceCurrency = Currency.Invalid;
		}
		else
		{
			result.PriceCurrency = (Currency)num;
		}
		unpacker.Read();
		result.PriceAmount = unpacker.LastReadData.AsInt64();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.OriginalPriceAmount = null;
		}
		else
		{
			long value = unpacker.LastReadData.AsInt64();
			result.OriginalPriceAmount = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SpecialDeal CommodityId={CommodityId} ExpiresAt={ExpiresAt} PriceCurrency={PriceCurrency} PriceAmount={PriceAmount} OriginalPriceAmount={OriginalPriceAmount}>";
	}
}

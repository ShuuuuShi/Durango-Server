using MsgPack;
using Shared.Economy;

namespace Messages;

public struct PriceRangePredicate
{
	public long? Min;

	public long? Max;

	public Currency Currency;

	public static void Pack(Packer packer, PriceRangePredicate val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (!val.Min.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Min.Value);
		}
		if (!val.Max.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Max.Value);
		}
		packer.Pack((int)val.Currency);
	}

	public static PriceRangePredicate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PriceRangePredicate result = default(PriceRangePredicate);
		if (unpacker.LastReadData.IsNil)
		{
			result.Min = null;
		}
		else
		{
			long value = unpacker.LastReadData.AsInt64();
			result.Min = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Max = null;
		}
		else
		{
			long value2 = unpacker.LastReadData.AsInt64();
			result.Max = value2;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 7 < num)
		{
			result.Currency = Currency.Invalid;
		}
		else
		{
			result.Currency = (Currency)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PriceRangePredicate Min={Min} Max={Max} Currency={Currency}>";
	}
}

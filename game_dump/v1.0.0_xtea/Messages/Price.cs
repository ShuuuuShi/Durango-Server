using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Price
{
	public int Amount;

	public Currency Currency;

	public static void Pack(Packer packer, Price val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.Amount);
		packer.Pack((int)val.Currency);
	}

	public static Price Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Price result = default(Price);
		result.Amount = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 1 < num)
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
		return $"<Price Amount={Amount} Currency={Currency}>";
	}
}

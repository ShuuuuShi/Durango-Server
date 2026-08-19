using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Cost
{
	public const uint TypeCode = 4023u;

	public Currency Currency;

	public long Amount;

	public static void Pack(Packer packer, Cost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4023u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Currency);
		packer.Pack(val.Amount);
	}

	public static Cost Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Cost result = default(Cost);
		if (num < 0 || 7 < num)
		{
			result.Currency = Currency.Invalid;
		}
		else
		{
			result.Currency = (Currency)num;
		}
		unpacker.Read();
		result.Amount = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<Cost Currency={Currency} Amount={Amount}>";
	}
}

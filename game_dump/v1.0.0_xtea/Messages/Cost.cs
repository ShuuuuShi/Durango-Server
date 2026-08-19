using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Cost
{
	public const uint TypeCode = 4023u;

	public Currency Currency;

	public int Amount;

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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Cost result = default(Cost);
		if (num < 0 || 1 < num)
		{
			result.Currency = Currency.Invalid;
		}
		else
		{
			result.Currency = (Currency)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Amount = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Cost Currency={Currency} Amount={Amount}>";
	}
}

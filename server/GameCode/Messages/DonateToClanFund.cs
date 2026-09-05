using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct DonateToClanFund
{
	public const uint TypeCode = 3679u;

	public Dictionary<Currency, long> Costs;

	public static void Pack(Packer packer, DonateToClanFund val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3679u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Costs == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Costs.Count);
		foreach (KeyValuePair<Currency, long> cost in val.Costs)
		{
			packer.Pack((int)cost.Key);
			packer.Pack(cost.Value);
		}
	}

	public static DonateToClanFund Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DonateToClanFund result = default(DonateToClanFund);
		result.Costs = new Dictionary<Currency, long>(num, default(CurrencyComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Currency key = ((num2 >= 0 && 7 >= num2) ? ((Currency)num2) : Currency.Invalid);
			unpacker.Read();
			long value = unpacker.LastReadData.AsInt64();
			result.Costs.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DonateToClanFund Costs={Costs}>";
	}
}

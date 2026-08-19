using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Costs
{
	public const uint TypeCode = 4024u;

	public Dictionary<Currency, long> _Costs;

	public static void Pack(Packer packer, Costs val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4024u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Costs == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val._Costs.Count);
		foreach (KeyValuePair<Currency, long> cost in val._Costs)
		{
			packer.Pack((int)cost.Key);
			packer.Pack(cost.Value);
		}
	}

	public static Costs Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Costs result = default(Costs);
		result._Costs = new Dictionary<Currency, long>(num, default(CurrencyComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Currency key = ((num2 >= 0 && 7 >= num2) ? ((Currency)num2) : Currency.Invalid);
			unpacker.Read();
			long value = unpacker.LastReadData.AsInt64();
			result._Costs.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Costs _Costs={_Costs}>";
	}
}

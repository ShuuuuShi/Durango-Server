using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Costs
{
	public const uint TypeCode = 4024u;

	public Dictionary<Currency, int> _Costs;

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
		foreach (KeyValuePair<Currency, int> cost in val._Costs)
		{
			packer.Pack((int)cost.Key);
			packer.Pack(cost.Value);
		}
	}

	public static Costs Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Costs result = default(Costs);
		result._Costs = new Dictionary<Currency, int>(num, default(CurrencyComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			Currency key = ((num2 >= 0 && 1 >= num2) ? ((Currency)num2) : Currency.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			result._Costs.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Costs _Costs={_Costs}>";
	}
}

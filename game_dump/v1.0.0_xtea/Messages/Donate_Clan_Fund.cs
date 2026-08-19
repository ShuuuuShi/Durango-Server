using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Donate_Clan_Fund
{
	public const uint TypeCode = 3679u;

	public Dictionary<Currency, int> Costs;

	public static void Pack(Packer packer, Donate_Clan_Fund val, bool hint = false)
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
		foreach (KeyValuePair<Currency, int> cost in val.Costs)
		{
			packer.Pack((int)cost.Key);
			packer.Pack(cost.Value);
		}
	}

	public static Donate_Clan_Fund Unpack(Unpacker unpacker)
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
		Donate_Clan_Fund result = default(Donate_Clan_Fund);
		result.Costs = new Dictionary<Currency, int>(num, default(CurrencyComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			Currency key = ((num2 >= 0 && 1 >= num2) ? ((Currency)num2) : Currency.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			result.Costs.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Donate_Clan_Fund Costs={Costs}>";
	}
}

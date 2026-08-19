using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Survival
{
	public const uint TypeCode = 182u;

	public ulong EntityId;

	public Gauge Life;

	public Dictionary<string, Gauge> Gauges;

	public static void Pack(Packer packer, Survival val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(182u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		if (val.Life == null)
		{
			packer.PackNull();
		}
		else
		{
			Gauge.PackTo(val.Life, packer);
		}
		if (val.Gauges == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Gauges.Count);
		foreach (KeyValuePair<string, Gauge> gauge in val.Gauges)
		{
			if (gauge.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(gauge.Key);
			}
			Gauge.PackTo(gauge.Value, packer);
		}
	}

	public static Survival Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Survival result = default(Survival);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Life = null;
		}
		else
		{
			Gauge life = Gauge.UnpackFrom(unpacker);
			result.Life = life;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Gauges = new Dictionary<string, Gauge>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData4)).AsString();
			unpacker.Read();
			Gauge value = Gauge.UnpackFrom(unpacker);
			result.Gauges.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Survival EntityId={EntityId} Life={Life} Gauges={Gauges}>";
	}
}

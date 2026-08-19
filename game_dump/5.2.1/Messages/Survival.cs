using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Survival
{
	public const uint TypeCode = 182u;

	public string EntityId;

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
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
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
		unpacker.Read();
		Survival result = default(Survival);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Life = null;
		}
		else
		{
			Gauge life = Gauge.UnpackFrom(unpacker);
			result.Life = life;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Gauges = new Dictionary<string, Gauge>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
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

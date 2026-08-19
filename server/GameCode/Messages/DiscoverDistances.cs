using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct DiscoverDistances
{
	public const uint TypeCode = 2309u;

	public Dictionary<byte, byte> PoiDistances;

	public Dictionary<string, byte> HoiDistances;

	public static void Pack(Packer packer, DiscoverDistances val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2309u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.PoiDistances == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.PoiDistances.Count);
			foreach (KeyValuePair<byte, byte> poiDistance in val.PoiDistances)
			{
				packer.Pack(poiDistance.Key);
				packer.Pack(poiDistance.Value);
			}
		}
		if (val.HoiDistances == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.HoiDistances.Count);
		foreach (KeyValuePair<string, byte> hoiDistance in val.HoiDistances)
		{
			if (hoiDistance.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(hoiDistance.Key);
			}
			packer.Pack(hoiDistance.Value);
		}
	}

	public static DiscoverDistances Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DiscoverDistances result = default(DiscoverDistances);
		result.PoiDistances = new Dictionary<byte, byte>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			byte key = unpacker.LastReadData.AsByte();
			unpacker.Read();
			byte value = unpacker.LastReadData.AsByte();
			result.PoiDistances.Add(key, value);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.HoiDistances = new Dictionary<string, byte>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key2 = unpacker.LastReadData.AsString();
			unpacker.Read();
			byte value2 = unpacker.LastReadData.AsByte();
			result.HoiDistances.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DiscoverDistances PoiDistances={PoiDistances} HoiDistances={HoiDistances}>";
	}
}

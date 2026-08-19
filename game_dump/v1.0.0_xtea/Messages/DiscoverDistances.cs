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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		DiscoverDistances result = default(DiscoverDistances);
		result.PoiDistances = new Dictionary<byte, byte>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			byte key = ((MessagePackObject)(ref lastReadData2)).AsByte();
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			byte value = ((MessagePackObject)(ref lastReadData3)).AsByte();
			result.PoiDistances.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.HoiDistances = new Dictionary<string, byte>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			string key2 = ((MessagePackObject)(ref lastReadData5)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			byte value2 = ((MessagePackObject)(ref lastReadData6)).AsByte();
			result.HoiDistances.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DiscoverDistances PoiDistances={PoiDistances} HoiDistances={HoiDistances}>";
	}
}

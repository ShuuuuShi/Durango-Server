using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct FarmingEncyclopedia
{
	public const uint TypeCode = 37126u;

	public Dictionary<string, FarmingEncyclopediaData> Data;

	public static void Pack(Packer packer, FarmingEncyclopedia val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(37126u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Data == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Data.Count);
		foreach (KeyValuePair<string, FarmingEncyclopediaData> datum in val.Data)
		{
			if (datum.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(datum.Key);
			}
			FarmingEncyclopediaData.Pack(packer, datum.Value);
		}
	}

	public static FarmingEncyclopedia Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		FarmingEncyclopedia result = default(FarmingEncyclopedia);
		result.Data = new Dictionary<string, FarmingEncyclopediaData>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			FarmingEncyclopediaData value = FarmingEncyclopediaData.Unpack(unpacker);
			result.Data.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FarmingEncyclopedia Data={Data}>";
	}
}

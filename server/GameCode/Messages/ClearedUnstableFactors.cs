using System.Collections.Generic;
using MsgPack;
using Shared.Region;

namespace Messages;

public struct ClearedUnstableFactors
{
	public const uint TypeCode = 240000u;

	public Dictionary<Pair<int, Biome>, int> _ClearedUnstableFactors;

	public static void Pack(Packer packer, ClearedUnstableFactors val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(240000u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._ClearedUnstableFactors == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val._ClearedUnstableFactors.Count);
		foreach (KeyValuePair<Pair<int, Biome>, int> clearedUnstableFactor in val._ClearedUnstableFactors)
		{
			packer.PackArrayHeader(2);
			packer.Pack(clearedUnstableFactor.Key.Item1);
			packer.Pack((int)clearedUnstableFactor.Key.Item2);
			packer.Pack(clearedUnstableFactor.Value);
		}
	}

	public static ClearedUnstableFactors Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ClearedUnstableFactors result = default(ClearedUnstableFactors);
		result._ClearedUnstableFactors = new Dictionary<Pair<int, Biome>, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			int item = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Biome item2 = ((num2 >= 0 && 15 >= num2) ? ((Biome)num2) : Biome.Invalid);
			Pair<int, Biome> key = new Pair<int, Biome>(item, item2);
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result._ClearedUnstableFactors.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ClearedUnstableFactors _ClearedUnstableFactors={_ClearedUnstableFactors}>";
	}
}

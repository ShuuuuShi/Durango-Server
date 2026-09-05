using MsgPack;
using Shared.Region;

namespace Messages;

public struct RecommendArchipelago
{
	public const uint TypeCode = 3012u;

	public int Level;

	public Biome Biome;

	public int? UnstableFactor;

	public static void Pack(Packer packer, RecommendArchipelago val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3012u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.Level);
		packer.Pack((int)val.Biome);
		if (!val.UnstableFactor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.UnstableFactor.Value);
		}
	}

	public static RecommendArchipelago Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RecommendArchipelago result = default(RecommendArchipelago);
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 15 < num)
		{
			result.Biome = Biome.Invalid;
		}
		else
		{
			result.Biome = (Biome)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.UnstableFactor = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.UnstableFactor = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RecommendArchipelago Level={Level} Biome={Biome} UnstableFactor={UnstableFactor}>";
	}
}

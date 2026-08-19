using MsgPack;

namespace Messages;

public struct FarmingEncyclopediaProgress
{
	public const uint TypeCode = 37127u;

	public string SeedPrototypeId;

	public FarmingEncyclopediaData Data;

	public static void Pack(Packer packer, FarmingEncyclopediaProgress val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(37127u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.SeedPrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SeedPrototypeId);
		}
		FarmingEncyclopediaData.Pack(packer, val.Data);
	}

	public static FarmingEncyclopediaProgress Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FarmingEncyclopediaProgress result = default(FarmingEncyclopediaProgress);
		result.SeedPrototypeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Data = FarmingEncyclopediaData.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<FarmingEncyclopediaProgress SeedPrototypeId={SeedPrototypeId} Data={Data}>";
	}
}

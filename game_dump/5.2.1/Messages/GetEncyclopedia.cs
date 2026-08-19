using MsgPack;
using Shared.Encyclopedia;

namespace Messages;

public struct GetEncyclopedia
{
	public const uint TypeCode = 37125u;

	public EncyclopediaType EncyclopediaCategory;

	public static void Pack(Packer packer, GetEncyclopedia val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(37125u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.EncyclopediaCategory);
	}

	public static GetEncyclopedia Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetEncyclopedia result = default(GetEncyclopedia);
		if (num < 0 || 0 < num)
		{
			result.EncyclopediaCategory = EncyclopediaType.Invalid;
		}
		else
		{
			result.EncyclopediaCategory = (EncyclopediaType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetEncyclopedia EncyclopediaCategory={EncyclopediaCategory}>";
	}
}

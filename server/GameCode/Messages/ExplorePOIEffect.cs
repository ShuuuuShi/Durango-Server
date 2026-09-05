using MsgPack;
using Shared.System;

namespace Messages;

public struct ExplorePOIEffect
{
	public const uint TypeCode = 2079u;

	public Shared.System.RewardEffect Type;

	public string PoiName;

	public static void Pack(Packer packer, ExplorePOIEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2079u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		packer.PackString(val.PoiName);
	}

	public static ExplorePOIEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ExplorePOIEffect result = default(ExplorePOIEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.PoiName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ExplorePOIEffect Type={Type} PoiName={PoiName}>";
	}
}

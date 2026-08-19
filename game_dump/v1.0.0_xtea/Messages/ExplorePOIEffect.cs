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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ExplorePOIEffect result = default(ExplorePOIEffect);
		if (num < 0 || 9 < num)
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

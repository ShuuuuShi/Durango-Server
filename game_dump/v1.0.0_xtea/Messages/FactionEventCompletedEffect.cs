using MsgPack;
using Shared.System;

namespace Messages;

public struct FactionEventCompletedEffect
{
	public const uint TypeCode = 2078u;

	public Shared.System.RewardEffect Type;

	public string FactionName;

	public static void Pack(Packer packer, FactionEventCompletedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2078u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		packer.PackString(val.FactionName);
	}

	public static FactionEventCompletedEffect Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionEventCompletedEffect result = default(FactionEventCompletedEffect);
		if (num < 0 || 9 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.FactionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<FactionEventCompletedEffect Type={Type} FactionName={FactionName}>";
	}
}

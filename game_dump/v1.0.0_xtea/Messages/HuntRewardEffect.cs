using MsgPack;
using Shared.System;

namespace Messages;

public struct HuntRewardEffect
{
	public const uint TypeCode = 2061u;

	public Shared.System.RewardEffect Type;

	public string TargetAnimal;

	public static void Pack(Packer packer, HuntRewardEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2061u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		packer.PackString(val.TargetAnimal);
	}

	public static HuntRewardEffect Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		HuntRewardEffect result = default(HuntRewardEffect);
		if (num < 0 || 9 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.TargetAnimal = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<HuntRewardEffect Type={Type} TargetAnimal={TargetAnimal}>";
	}
}

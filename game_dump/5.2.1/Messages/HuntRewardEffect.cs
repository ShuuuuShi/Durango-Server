using MsgPack;
using Shared.System;

namespace Messages;

public struct HuntRewardEffect
{
	public const uint TypeCode = 2061u;

	public Shared.System.RewardEffect Type;

	public string TargetAnimal;

	public int TargetEntityType;

	public static void Pack(Packer packer, HuntRewardEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2061u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		packer.PackString(val.TargetAnimal);
		packer.Pack(val.TargetEntityType);
	}

	public static HuntRewardEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		HuntRewardEffect result = default(HuntRewardEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.TargetAnimal = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.TargetEntityType = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<HuntRewardEffect Type={Type} TargetAnimal={TargetAnimal} TargetEntityType={TargetEntityType}>";
	}
}

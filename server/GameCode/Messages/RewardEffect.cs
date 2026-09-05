using MsgPack;
using Shared.System;

namespace Messages;

public struct RewardEffect
{
	public Shared.System.RewardEffect Type;

	public static void Pack(Packer packer, RewardEffect val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		packer.Pack((int)val.Type);
	}

	public static RewardEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RewardEffect result = default(RewardEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RewardEffect Type={Type}>";
	}
}

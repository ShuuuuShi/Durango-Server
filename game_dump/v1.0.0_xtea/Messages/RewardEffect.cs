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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		RewardEffect result = default(RewardEffect);
		if (num < 0 || 9 < num)
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

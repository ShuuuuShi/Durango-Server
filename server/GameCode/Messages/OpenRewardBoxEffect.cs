using MsgPack;
using Shared.System;

namespace Messages;

public struct OpenRewardBoxEffect
{
	public const uint TypeCode = 29875326u;

	public Shared.System.RewardEffect Type;

	public static void Pack(Packer packer, OpenRewardBoxEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(29875326u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Type);
	}

	public static OpenRewardBoxEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		OpenRewardBoxEffect result = default(OpenRewardBoxEffect);
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
		return $"<OpenRewardBoxEffect Type={Type}>";
	}
}

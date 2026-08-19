using MsgPack;
using Shared.System;

namespace Messages;

public struct RankingRewardEffect
{
	public const uint TypeCode = 20871u;

	public Shared.System.RewardEffect Type;

	public static void Pack(Packer packer, RankingRewardEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(20871u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Type);
	}

	public static RankingRewardEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RankingRewardEffect result = default(RankingRewardEffect);
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
		return $"<RankingRewardEffect Type={Type}>";
	}
}

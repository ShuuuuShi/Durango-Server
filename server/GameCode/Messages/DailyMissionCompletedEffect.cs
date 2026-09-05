using MsgPack;
using Shared.System;

namespace Messages;

public struct DailyMissionCompletedEffect
{
	public const uint TypeCode = 19843572u;

	public Shared.System.RewardEffect Type;

	public static void Pack(Packer packer, DailyMissionCompletedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(19843572u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Type);
	}

	public static DailyMissionCompletedEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DailyMissionCompletedEffect result = default(DailyMissionCompletedEffect);
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
		return $"<DailyMissionCompletedEffect Type={Type}>";
	}
}

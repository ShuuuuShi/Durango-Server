using System.Collections.Generic;
using MsgPack;
using Shared.Attendance;

namespace Messages;

public struct TodayAttendanceRewards
{
	public const uint TypeCode = 1097851u;

	public Dictionary<CategoryType, TodayAttendanceReward> Rewards;

	public static void Pack(Packer packer, TodayAttendanceRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1097851u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Rewards == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Rewards.Count);
		foreach (KeyValuePair<CategoryType, TodayAttendanceReward> reward in val.Rewards)
		{
			packer.Pack((int)reward.Key);
			TodayAttendanceReward.Pack(packer, reward.Value);
		}
	}

	public static TodayAttendanceRewards Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		TodayAttendanceRewards result = default(TodayAttendanceRewards);
		result.Rewards = new Dictionary<CategoryType, TodayAttendanceReward>(num, default(CategoryTypeComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			CategoryType key = ((num2 >= 1 && 7 >= num2) ? ((CategoryType)num2) : CategoryType.Invalid);
			unpacker.Read();
			TodayAttendanceReward value = TodayAttendanceReward.Unpack(unpacker);
			result.Rewards.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TodayAttendanceRewards Rewards={Rewards}>";
	}
}

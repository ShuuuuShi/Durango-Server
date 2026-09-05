using MsgPack;
using Shared.Attendance;

namespace Messages;

public struct AttendanceRewards
{
	public const uint TypeCode = 1097853u;

	public CategoryType Category;

	public AttendanceReward[] Rewards;

	public AttendanceReward[] Appendices;

	public static void Pack(Packer packer, AttendanceRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(1097853u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Category);
		if (val.Rewards == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Rewards.Length);
			for (int i = 0; i < val.Rewards.Length; i++)
			{
				AttendanceReward.Pack(packer, val.Rewards[i]);
			}
		}
		if (val.Appendices == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Appendices.Length);
		for (int j = 0; j < val.Appendices.Length; j++)
		{
			AttendanceReward.Pack(packer, val.Appendices[j]);
		}
	}

	public static AttendanceRewards Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AttendanceRewards result = default(AttendanceRewards);
		if (num < 1 || 7 < num)
		{
			result.Category = CategoryType.Invalid;
		}
		else
		{
			result.Category = (CategoryType)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Rewards = new AttendanceReward[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref AttendanceReward reference = ref result.Rewards[i];
			reference = AttendanceReward.Unpack(unpacker);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.Appendices = new AttendanceReward[num3];
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			ref AttendanceReward reference2 = ref result.Appendices[j];
			reference2 = AttendanceReward.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AttendanceRewards Category={Category} Rewards={Rewards} Appendices={Appendices}>";
	}
}

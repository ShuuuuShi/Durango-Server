using MsgPack;
using Shared.Attendance;

namespace Messages;

public struct GiveAttendanceReward
{
	public const uint TypeCode = 1097854u;

	public CategoryType Category;

	public int RewardNumber;

	public bool IsRestore;

	public static void Pack(Packer packer, GiveAttendanceReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(1097854u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Category);
		packer.Pack(val.RewardNumber);
		packer.Pack(val.IsRestore);
	}

	public static GiveAttendanceReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GiveAttendanceReward result = default(GiveAttendanceReward);
		if (num < 1 || 7 < num)
		{
			result.Category = CategoryType.Invalid;
		}
		else
		{
			result.Category = (CategoryType)num;
		}
		unpacker.Read();
		result.RewardNumber = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.IsRestore = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<GiveAttendanceReward Category={Category} RewardNumber={RewardNumber} IsRestore={IsRestore}>";
	}
}

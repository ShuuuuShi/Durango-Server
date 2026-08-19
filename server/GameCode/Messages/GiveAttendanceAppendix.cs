using MsgPack;
using Shared.Attendance;

namespace Messages;

public struct GiveAttendanceAppendix
{
	public const uint TypeCode = 1097855u;

	public CategoryType Category;

	public int SelectedReward;

	public static void Pack(Packer packer, GiveAttendanceAppendix val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(1097855u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Category);
		packer.Pack(val.SelectedReward);
	}

	public static GiveAttendanceAppendix Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GiveAttendanceAppendix result = default(GiveAttendanceAppendix);
		if (num < 1 || 7 < num)
		{
			result.Category = CategoryType.Invalid;
		}
		else
		{
			result.Category = (CategoryType)num;
		}
		unpacker.Read();
		result.SelectedReward = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<GiveAttendanceAppendix Category={Category} SelectedReward={SelectedReward}>";
	}
}

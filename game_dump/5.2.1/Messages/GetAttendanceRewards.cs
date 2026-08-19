using MsgPack;
using Shared.Attendance;

namespace Messages;

public struct GetAttendanceRewards
{
	public const uint TypeCode = 1097852u;

	public CategoryType Category;

	public static void Pack(Packer packer, GetAttendanceRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1097852u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Category);
	}

	public static GetAttendanceRewards Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetAttendanceRewards result = default(GetAttendanceRewards);
		if (num < 1 || 7 < num)
		{
			result.Category = CategoryType.Invalid;
		}
		else
		{
			result.Category = (CategoryType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetAttendanceRewards Category={Category}>";
	}
}

using MsgPack;

namespace Messages;

public struct RegionCoOpTodoCompleted
{
	public const uint TypeCode = 241004u;

	public string RegionId;

	public string CoOpId;

	public static void Pack(Packer packer, RegionCoOpTodoCompleted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(241004u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		if (val.CoOpId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CoOpId);
		}
	}

	public static RegionCoOpTodoCompleted Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RegionCoOpTodoCompleted result = default(RegionCoOpTodoCompleted);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.CoOpId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RegionCoOpTodoCompleted RegionId={RegionId} CoOpId={CoOpId}>";
	}
}

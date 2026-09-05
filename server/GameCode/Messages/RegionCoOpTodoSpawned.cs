using MsgPack;

namespace Messages;

public struct RegionCoOpTodoSpawned
{
	public const uint TypeCode = 241002u;

	public string RegionId;

	public RegionCoOpTodo CoOp;

	public static void Pack(Packer packer, RegionCoOpTodoSpawned val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(241002u);
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
		RegionCoOpTodo.Pack(packer, val.CoOp);
	}

	public static RegionCoOpTodoSpawned Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RegionCoOpTodoSpawned result = default(RegionCoOpTodoSpawned);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.CoOp = RegionCoOpTodo.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RegionCoOpTodoSpawned RegionId={RegionId} CoOp={CoOp}>";
	}
}

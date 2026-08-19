using MsgPack;

namespace Messages;

public struct BuildTodo
{
	public const uint TypeCode = 3521u;

	public string BlueprintId;

	public static void Pack(Packer packer, BuildTodo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3521u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
	}

	public static BuildTodo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BuildTodo result = default(BuildTodo);
		result.BlueprintId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<BuildTodo BlueprintId={BlueprintId}>";
	}
}

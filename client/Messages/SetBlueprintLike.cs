using MsgPack;

namespace Messages;

public struct SetBlueprintLike
{
	public const uint TypeCode = 3801u;

	public string BlueprintId;

	public bool Like;

	public static void Pack(Packer packer, SetBlueprintLike val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3801u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
		packer.Pack(val.Like);
	}

	public static SetBlueprintLike Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetBlueprintLike result = default(SetBlueprintLike);
		result.BlueprintId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Like = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<SetBlueprintLike BlueprintId={BlueprintId} Like={Like}>";
	}
}

using MsgPack;

namespace Messages;

public struct Block
{
	public const uint TypeCode = 4016u;

	public string EntityId;

	public static void Pack(Packer packer, Block val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4016u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static Block Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Block result = default(Block);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<Block EntityId=" + EntityId + ">";
	}
}

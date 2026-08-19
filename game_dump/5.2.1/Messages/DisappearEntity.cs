using MsgPack;

namespace Messages;

public struct DisappearEntity
{
	public const uint TypeCode = 101u;

	public string EntityId;

	public static void Pack(Packer packer, DisappearEntity val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(101u);
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

	public static DisappearEntity Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DisappearEntity result = default(DisappearEntity);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<DisappearEntity EntityId=" + EntityId + ">";
	}
}

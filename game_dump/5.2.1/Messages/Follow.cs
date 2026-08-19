using MsgPack;

namespace Messages;

public struct Follow
{
	public const uint TypeCode = 2401u;

	public string EntityId;

	public static void Pack(Packer packer, Follow val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2401u);
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

	public static Follow Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Follow result = default(Follow);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<Follow EntityId=" + EntityId + ">";
	}
}

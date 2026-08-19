using MsgPack;

namespace Messages;

public struct RefuseFriendRequest
{
	public const uint TypeCode = 1451216u;

	public string EntityId;

	public static void Pack(Packer packer, RefuseFriendRequest val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1451216u);
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

	public static RefuseFriendRequest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RefuseFriendRequest result = default(RefuseFriendRequest);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RefuseFriendRequest EntityId=" + EntityId + ">";
	}
}

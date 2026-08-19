using MsgPack;

namespace Messages;

public struct AcceptFriendRequest
{
	public const uint TypeCode = 1451215u;

	public string EntityId;

	public static void Pack(Packer packer, AcceptFriendRequest val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1451215u);
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

	public static AcceptFriendRequest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptFriendRequest result = default(AcceptFriendRequest);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<AcceptFriendRequest EntityId=" + EntityId + ">";
	}
}

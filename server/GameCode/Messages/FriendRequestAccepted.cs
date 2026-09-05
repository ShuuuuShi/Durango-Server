using MsgPack;

namespace Messages;

public struct FriendRequestAccepted
{
	public const uint TypeCode = 1451219u;

	public string EntityId;

	public static void Pack(Packer packer, FriendRequestAccepted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1451219u);
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

	public static FriendRequestAccepted Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FriendRequestAccepted result = default(FriendRequestAccepted);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<FriendRequestAccepted EntityId={EntityId}>";
	}
}

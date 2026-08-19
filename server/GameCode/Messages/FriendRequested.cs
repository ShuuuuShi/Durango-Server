using MsgPack;

namespace Messages;

public struct FriendRequested
{
	public const uint TypeCode = 1451218u;

	public string EntityId;

	public static void Pack(Packer packer, FriendRequested val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1451218u);
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

	public static FriendRequested Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FriendRequested result = default(FriendRequested);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<FriendRequested EntityId={EntityId}>";
	}
}

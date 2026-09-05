using MsgPack;

namespace Messages;

public struct CancelFriendRequest
{
	public const uint TypeCode = 1451220u;

	public string EntityId;

	public static void Pack(Packer packer, CancelFriendRequest val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1451220u);
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

	public static CancelFriendRequest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CancelFriendRequest result = default(CancelFriendRequest);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<CancelFriendRequest EntityId={EntityId}>";
	}
}

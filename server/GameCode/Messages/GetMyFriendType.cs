using MsgPack;

namespace Messages;

public struct GetMyFriendType
{
	public const uint TypeCode = 78209743u;

	public string EntityId;

	public static void Pack(Packer packer, GetMyFriendType val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(78209743u);
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

	public static GetMyFriendType Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetMyFriendType result = default(GetMyFriendType);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetMyFriendType EntityId={EntityId}>";
	}
}

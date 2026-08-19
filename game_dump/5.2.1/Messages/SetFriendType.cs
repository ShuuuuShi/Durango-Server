using MsgPack;
using Shared.Player;

namespace Messages;

public struct SetFriendType
{
	public const uint TypeCode = 908134u;

	public string EntityId;

	public Shared.Player.FriendType Type;

	public static void Pack(Packer packer, SetFriendType val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(908134u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack((int)val.Type);
	}

	public static SetFriendType Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetFriendType result = default(SetFriendType);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 1 < num)
		{
			result.Type = Shared.Player.FriendType.Invalid;
		}
		else
		{
			result.Type = (Shared.Player.FriendType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetFriendType EntityId={EntityId} Type={Type}>";
	}
}

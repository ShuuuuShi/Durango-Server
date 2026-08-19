using MsgPack;
using Shared.Player;

namespace Messages;

public struct FriendType
{
	public const uint TypeCode = 78209744u;

	public Shared.Player.FriendType _FriendType;

	public static void Pack(Packer packer, FriendType val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(78209744u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val._FriendType);
	}

	public static FriendType Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		FriendType result = default(FriendType);
		if (num < 0 || 1 < num)
		{
			result._FriendType = Shared.Player.FriendType.Invalid;
		}
		else
		{
			result._FriendType = (Shared.Player.FriendType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FriendType _FriendType={_FriendType}>";
	}
}

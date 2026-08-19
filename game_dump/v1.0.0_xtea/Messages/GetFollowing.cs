using MsgPack;

namespace Messages;

public struct GetFollowing
{
	public const uint TypeCode = 2402u;

	public static void Pack(Packer packer, GetFollowing val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2402u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetFollowing Unpack(Unpacker unpacker)
	{
		GetFollowing result = default(GetFollowing);
		return result;
	}

	public override string ToString()
	{
		return "<GetFollowing>";
	}
}

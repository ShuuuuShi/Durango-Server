using MsgPack;

namespace Messages;

public struct RegionMovedByExpiration
{
	public const uint TypeCode = 2425u;

	public static void Pack(Packer packer, RegionMovedByExpiration val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2425u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RegionMovedByExpiration Unpack(Unpacker unpacker)
	{
		RegionMovedByExpiration result = default(RegionMovedByExpiration);
		return result;
	}

	public override string ToString()
	{
		return "<RegionMovedByExpiration>";
	}
}

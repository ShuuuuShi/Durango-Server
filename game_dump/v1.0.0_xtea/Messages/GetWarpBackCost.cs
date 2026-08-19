using MsgPack;

namespace Messages;

public struct GetWarpBackCost
{
	public const uint TypeCode = 2109u;

	public static void Pack(Packer packer, GetWarpBackCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2109u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetWarpBackCost Unpack(Unpacker unpacker)
	{
		GetWarpBackCost result = default(GetWarpBackCost);
		return result;
	}

	public override string ToString()
	{
		return "<GetWarpBackCost>";
	}
}

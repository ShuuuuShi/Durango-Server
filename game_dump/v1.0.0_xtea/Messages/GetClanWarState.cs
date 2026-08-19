using MsgPack;

namespace Messages;

public struct GetClanWarState
{
	public const uint TypeCode = 3673u;

	public static void Pack(Packer packer, GetClanWarState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3673u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanWarState Unpack(Unpacker unpacker)
	{
		GetClanWarState result = default(GetClanWarState);
		return result;
	}

	public override string ToString()
	{
		return "<GetClanWarState>";
	}
}

using MsgPack;

namespace Messages;

public struct Keepalive
{
	public const uint TypeCode = 254u;

	public static void Pack(Packer packer, Keepalive val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(254u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Keepalive Unpack(Unpacker unpacker)
	{
		Keepalive result = default(Keepalive);
		return result;
	}

	public override string ToString()
	{
		return "<Keepalive>";
	}
}

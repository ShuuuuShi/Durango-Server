using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct UnmountAirBalloon
{
	public const uint TypeCode = 135867u;

	public static void Pack(Packer packer, UnmountAirBalloon val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(135867u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static UnmountAirBalloon Unpack(Unpacker unpacker)
	{
		UnmountAirBalloon result = default(UnmountAirBalloon);
		return result;
	}

	public override string ToString()
	{
		return "<UnmountAirBalloon>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TimedOut
{
	public const uint TypeCode = 3650u;

	public static void Pack(Packer packer, TimedOut val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3650u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static TimedOut Unpack(Unpacker unpacker)
	{
		return default(TimedOut);
	}

	public override string ToString()
	{
		return "<TimedOut>";
	}
}

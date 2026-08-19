using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02PVPAnnounceLeave
{
	public const uint TypeCode = 222208u;

	public static void Pack(Packer packer, S02PVPAnnounceLeave val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222208u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02PVPAnnounceLeave Unpack(Unpacker unpacker)
	{
		return default(S02PVPAnnounceLeave);
	}

	public override string ToString()
	{
		return "<S02PVPAnnounceLeave>";
	}
}

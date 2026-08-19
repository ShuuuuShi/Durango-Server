using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02PVPRefresh
{
	public const uint TypeCode = 222207u;

	public static void Pack(Packer packer, S02PVPRefresh val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222207u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02PVPRefresh Unpack(Unpacker unpacker)
	{
		return default(S02PVPRefresh);
	}

	public override string ToString()
	{
		return "<S02PVPRefresh>";
	}
}

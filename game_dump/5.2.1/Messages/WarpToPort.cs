using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct WarpToPort
{
	public const uint TypeCode = 9081241u;

	public static void Pack(Packer packer, WarpToPort val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(9081241u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static WarpToPort Unpack(Unpacker unpacker)
	{
		return default(WarpToPort);
	}

	public override string ToString()
	{
		return "<WarpToPort>";
	}
}

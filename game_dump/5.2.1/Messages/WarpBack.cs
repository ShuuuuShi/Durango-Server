using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct WarpBack
{
	public const uint TypeCode = 2110u;

	public static void Pack(Packer packer, WarpBack val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2110u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static WarpBack Unpack(Unpacker unpacker)
	{
		return default(WarpBack);
	}

	public override string ToString()
	{
		return "<WarpBack>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Mount
{
	public const uint TypeCode = 802u;

	public static void Pack(Packer packer, Mount val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(802u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Mount Unpack(Unpacker unpacker)
	{
		return default(Mount);
	}

	public override string ToString()
	{
		return "<Mount>";
	}
}

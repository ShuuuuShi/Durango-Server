using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct WarpToNextArchipelagoRegion
{
	public const uint TypeCode = 2035u;

	public static void Pack(Packer packer, WarpToNextArchipelagoRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2035u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static WarpToNextArchipelagoRegion Unpack(Unpacker unpacker)
	{
		return default(WarpToNextArchipelagoRegion);
	}

	public override string ToString()
	{
		return "<WarpToNextArchipelagoRegion>";
	}
}

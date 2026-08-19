using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RequestArchipelagoRegionClear
{
	public const uint TypeCode = 240002u;

	public static void Pack(Packer packer, RequestArchipelagoRegionClear val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(240002u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RequestArchipelagoRegionClear Unpack(Unpacker unpacker)
	{
		return default(RequestArchipelagoRegionClear);
	}

	public override string ToString()
	{
		return "<RequestArchipelagoRegionClear>";
	}
}

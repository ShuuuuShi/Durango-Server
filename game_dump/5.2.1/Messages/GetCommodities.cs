using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetCommodities
{
	public const uint TypeCode = 856700u;

	public static void Pack(Packer packer, GetCommodities val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(856700u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetCommodities Unpack(Unpacker unpacker)
	{
		return default(GetCommodities);
	}

	public override string ToString()
	{
		return "<GetCommodities>";
	}
}

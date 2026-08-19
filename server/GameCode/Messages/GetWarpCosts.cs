using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetWarpCosts
{
	public const uint TypeCode = 2106u;

	public static void Pack(Packer packer, GetWarpCosts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2106u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetWarpCosts Unpack(Unpacker unpacker)
	{
		GetWarpCosts result = default(GetWarpCosts);
		return result;
	}

	public override string ToString()
	{
		return "<GetWarpCosts>";
	}
}

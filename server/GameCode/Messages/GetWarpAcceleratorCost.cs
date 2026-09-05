using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetWarpAcceleratorCost
{
	public const uint TypeCode = 21112519u;

	public static void Pack(Packer packer, GetWarpAcceleratorCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(21112519u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetWarpAcceleratorCost Unpack(Unpacker unpacker)
	{
		GetWarpAcceleratorCost result = default(GetWarpAcceleratorCost);
		return result;
	}

	public override string ToString()
	{
		return "<GetWarpAcceleratorCost>";
	}
}

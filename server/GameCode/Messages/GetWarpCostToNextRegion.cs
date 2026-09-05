using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetWarpCostToNextRegion
{
	public const uint TypeCode = 12033u;

	public static void Pack(Packer packer, GetWarpCostToNextRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(12033u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetWarpCostToNextRegion Unpack(Unpacker unpacker)
	{
		GetWarpCostToNextRegion result = default(GetWarpCostToNextRegion);
		return result;
	}

	public override string ToString()
	{
		return "<GetWarpCostToNextRegion>";
	}
}

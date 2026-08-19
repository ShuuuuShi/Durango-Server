using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TravelToRandomPersonalRegion
{
	public const uint TypeCode = 20314u;

	public static void Pack(Packer packer, TravelToRandomPersonalRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(20314u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static TravelToRandomPersonalRegion Unpack(Unpacker unpacker)
	{
		TravelToRandomPersonalRegion result = default(TravelToRandomPersonalRegion);
		return result;
	}

	public override string ToString()
	{
		return "<TravelToRandomPersonalRegion>";
	}
}

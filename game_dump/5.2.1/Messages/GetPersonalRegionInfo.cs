using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetPersonalRegionInfo
{
	public const uint TypeCode = 20420u;

	public static void Pack(Packer packer, GetPersonalRegionInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(20420u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetPersonalRegionInfo Unpack(Unpacker unpacker)
	{
		return default(GetPersonalRegionInfo);
	}

	public override string ToString()
	{
		return "<GetPersonalRegionInfo>";
	}
}

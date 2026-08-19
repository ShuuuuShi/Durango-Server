using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetResistanceExpCaps
{
	public const uint TypeCode = 349378781u;

	public static void Pack(Packer packer, GetResistanceExpCaps val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(349378781u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetResistanceExpCaps Unpack(Unpacker unpacker)
	{
		return default(GetResistanceExpCaps);
	}

	public override string ToString()
	{
		return "<GetResistanceExpCaps>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Depart
{
	public const uint TypeCode = 2448u;

	public static void Pack(Packer packer, Depart val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2448u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Depart Unpack(Unpacker unpacker)
	{
		return default(Depart);
	}

	public override string ToString()
	{
		return "<Depart>";
	}
}

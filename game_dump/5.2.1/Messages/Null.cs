using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Null
{
	public const uint TypeCode = 255u;

	public static void Pack(Packer packer, Null val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(255u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Null Unpack(Unpacker unpacker)
	{
		return default(Null);
	}

	public override string ToString()
	{
		return "<Null>";
	}
}

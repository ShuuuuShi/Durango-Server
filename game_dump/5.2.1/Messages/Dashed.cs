using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Dashed
{
	public const uint TypeCode = 2491u;

	public static void Pack(Packer packer, Dashed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2491u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Dashed Unpack(Unpacker unpacker)
	{
		return default(Dashed);
	}

	public override string ToString()
	{
		return "<Dashed>";
	}
}

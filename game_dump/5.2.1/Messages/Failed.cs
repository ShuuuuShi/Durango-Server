using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Failed
{
	public const uint TypeCode = 1232u;

	public static void Pack(Packer packer, Failed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1232u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Failed Unpack(Unpacker unpacker)
	{
		return default(Failed);
	}

	public override string ToString()
	{
		return "<Failed>";
	}
}

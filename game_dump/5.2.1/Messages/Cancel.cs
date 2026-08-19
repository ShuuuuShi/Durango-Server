using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Cancel
{
	public const uint TypeCode = 2036u;

	public static void Pack(Packer packer, Cancel val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2036u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Cancel Unpack(Unpacker unpacker)
	{
		return default(Cancel);
	}

	public override string ToString()
	{
		return "<Cancel>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Ready
{
	public const uint TypeCode = 20u;

	public static void Pack(Packer packer, Ready val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(20u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Ready Unpack(Unpacker unpacker)
	{
		return default(Ready);
	}

	public override string ToString()
	{
		return "<Ready>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ResetAccessory
{
	public const uint TypeCode = 9823460u;

	public static void Pack(Packer packer, ResetAccessory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(9823460u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ResetAccessory Unpack(Unpacker unpacker)
	{
		return default(ResetAccessory);
	}

	public override string ToString()
	{
		return "<ResetAccessory>";
	}
}

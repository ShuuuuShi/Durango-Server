using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetBlocklist
{
	public const uint TypeCode = 4018u;

	public static void Pack(Packer packer, GetBlocklist val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(4018u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetBlocklist Unpack(Unpacker unpacker)
	{
		GetBlocklist result = default(GetBlocklist);
		return result;
	}

	public override string ToString()
	{
		return "<GetBlocklist>";
	}
}

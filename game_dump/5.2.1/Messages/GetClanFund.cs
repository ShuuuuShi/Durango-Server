using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetClanFund
{
	public const uint TypeCode = 3678u;

	public static void Pack(Packer packer, GetClanFund val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3678u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanFund Unpack(Unpacker unpacker)
	{
		return default(GetClanFund);
	}

	public override string ToString()
	{
		return "<GetClanFund>";
	}
}

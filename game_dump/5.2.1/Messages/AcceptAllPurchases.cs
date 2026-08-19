using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AcceptAllPurchases
{
	public const uint TypeCode = 5247810u;

	public static void Pack(Packer packer, AcceptAllPurchases val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(5247810u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static AcceptAllPurchases Unpack(Unpacker unpacker)
	{
		return default(AcceptAllPurchases);
	}

	public override string ToString()
	{
		return "<AcceptAllPurchases>";
	}
}

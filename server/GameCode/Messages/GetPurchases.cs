using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetPurchases
{
	public const uint TypeCode = 510397u;

	public static void Pack(Packer packer, GetPurchases val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(510397u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetPurchases Unpack(Unpacker unpacker)
	{
		GetPurchases result = default(GetPurchases);
		return result;
	}

	public override string ToString()
	{
		return "<GetPurchases>";
	}
}

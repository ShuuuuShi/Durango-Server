using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetAcceptableSubPurchases
{
	public const uint TypeCode = 259674u;

	public static void Pack(Packer packer, GetAcceptableSubPurchases val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(259674u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetAcceptableSubPurchases Unpack(Unpacker unpacker)
	{
		GetAcceptableSubPurchases result = default(GetAcceptableSubPurchases);
		return result;
	}

	public override string ToString()
	{
		return "<GetAcceptableSubPurchases>";
	}
}

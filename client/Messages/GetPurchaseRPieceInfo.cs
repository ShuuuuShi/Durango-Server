using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetPurchaseRPieceInfo
{
	public const uint TypeCode = 19021101u;

	public static void Pack(Packer packer, GetPurchaseRPieceInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(19021101u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetPurchaseRPieceInfo Unpack(Unpacker unpacker)
	{
		GetPurchaseRPieceInfo result = default(GetPurchaseRPieceInfo);
		return result;
	}

	public override string ToString()
	{
		return "<GetPurchaseRPieceInfo>";
	}
}

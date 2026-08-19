using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PurchaseRPiece
{
	public const uint TypeCode = 19021103u;

	public static void Pack(Packer packer, PurchaseRPiece val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(19021103u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static PurchaseRPiece Unpack(Unpacker unpacker)
	{
		return default(PurchaseRPiece);
	}

	public override string ToString()
	{
		return "<PurchaseRPiece>";
	}
}

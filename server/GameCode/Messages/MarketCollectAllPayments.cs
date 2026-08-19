using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MarketCollectAllPayments
{
	public const uint TypeCode = 5102u;

	public static void Pack(Packer packer, MarketCollectAllPayments val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(5102u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static MarketCollectAllPayments Unpack(Unpacker unpacker)
	{
		MarketCollectAllPayments result = default(MarketCollectAllPayments);
		return result;
	}

	public override string ToString()
	{
		return "<MarketCollectAllPayments>";
	}
}

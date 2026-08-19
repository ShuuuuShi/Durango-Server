using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetOffers
{
	public const uint TypeCode = 3498u;

	public static void Pack(Packer packer, GetOffers val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3498u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetOffers Unpack(Unpacker unpacker)
	{
		return default(GetOffers);
	}

	public override string ToString()
	{
		return "<GetOffers>";
	}
}

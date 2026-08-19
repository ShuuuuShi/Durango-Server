using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetSpecialDeals
{
	public const uint TypeCode = 259680u;

	public static void Pack(Packer packer, GetSpecialDeals val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(259680u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetSpecialDeals Unpack(Unpacker unpacker)
	{
		return default(GetSpecialDeals);
	}

	public override string ToString()
	{
		return "<GetSpecialDeals>";
	}
}

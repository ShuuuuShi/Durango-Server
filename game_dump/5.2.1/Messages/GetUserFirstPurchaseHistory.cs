using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetUserFirstPurchaseHistory
{
	public const uint TypeCode = 856720u;

	public static void Pack(Packer packer, GetUserFirstPurchaseHistory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(856720u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetUserFirstPurchaseHistory Unpack(Unpacker unpacker)
	{
		return default(GetUserFirstPurchaseHistory);
	}

	public override string ToString()
	{
		return "<GetUserFirstPurchaseHistory>";
	}
}

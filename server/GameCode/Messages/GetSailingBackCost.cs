using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetSailingBackCost
{
	public const uint TypeCode = 3110u;

	public static void Pack(Packer packer, GetSailingBackCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3110u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetSailingBackCost Unpack(Unpacker unpacker)
	{
		GetSailingBackCost result = default(GetSailingBackCost);
		return result;
	}

	public override string ToString()
	{
		return "<GetSailingBackCost>";
	}
}

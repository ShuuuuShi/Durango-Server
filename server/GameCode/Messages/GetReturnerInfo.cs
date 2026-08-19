using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetReturnerInfo
{
	public const uint TypeCode = 3450983u;

	public static void Pack(Packer packer, GetReturnerInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3450983u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetReturnerInfo Unpack(Unpacker unpacker)
	{
		GetReturnerInfo result = default(GetReturnerInfo);
		return result;
	}

	public override string ToString()
	{
		return "<GetReturnerInfo>";
	}
}

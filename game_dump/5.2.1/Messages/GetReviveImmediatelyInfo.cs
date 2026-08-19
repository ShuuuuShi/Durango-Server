using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetReviveImmediatelyInfo
{
	public const uint TypeCode = 210101u;

	public static void Pack(Packer packer, GetReviveImmediatelyInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(210101u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetReviveImmediatelyInfo Unpack(Unpacker unpacker)
	{
		return default(GetReviveImmediatelyInfo);
	}

	public override string ToString()
	{
		return "<GetReviveImmediatelyInfo>";
	}
}

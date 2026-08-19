using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetTitles
{
	public const uint TypeCode = 2044u;

	public static void Pack(Packer packer, GetTitles val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2044u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetTitles Unpack(Unpacker unpacker)
	{
		return default(GetTitles);
	}

	public override string ToString()
	{
		return "<GetTitles>";
	}
}

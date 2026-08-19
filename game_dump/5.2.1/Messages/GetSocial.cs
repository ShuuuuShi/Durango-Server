using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetSocial
{
	public const uint TypeCode = 2402u;

	public static void Pack(Packer packer, GetSocial val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2402u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetSocial Unpack(Unpacker unpacker)
	{
		return default(GetSocial);
	}

	public override string ToString()
	{
		return "<GetSocial>";
	}
}

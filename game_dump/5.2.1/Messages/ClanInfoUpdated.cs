using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ClanInfoUpdated
{
	public const uint TypeCode = 698346u;

	public static void Pack(Packer packer, ClanInfoUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(698346u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ClanInfoUpdated Unpack(Unpacker unpacker)
	{
		return default(ClanInfoUpdated);
	}

	public override string ToString()
	{
		return "<ClanInfoUpdated>";
	}
}

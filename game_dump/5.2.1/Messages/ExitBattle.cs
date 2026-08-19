using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ExitBattle
{
	public const uint TypeCode = 3496u;

	public static void Pack(Packer packer, ExitBattle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3496u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ExitBattle Unpack(Unpacker unpacker)
	{
		return default(ExitBattle);
	}

	public override string ToString()
	{
		return "<ExitBattle>";
	}
}

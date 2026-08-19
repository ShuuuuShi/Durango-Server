using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetAllySlots
{
	public const uint TypeCode = 9138745u;

	public static void Pack(Packer packer, GetAllySlots val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(9138745u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetAllySlots Unpack(Unpacker unpacker)
	{
		return default(GetAllySlots);
	}

	public override string ToString()
	{
		return "<GetAllySlots>";
	}
}

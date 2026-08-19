using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetParty
{
	public const uint TypeCode = 20001u;

	public static void Pack(Packer packer, GetParty val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(20001u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetParty Unpack(Unpacker unpacker)
	{
		return default(GetParty);
	}

	public override string ToString()
	{
		return "<GetParty>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MakeParty
{
	public const uint TypeCode = 20003u;

	public static void Pack(Packer packer, MakeParty val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(20003u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static MakeParty Unpack(Unpacker unpacker)
	{
		return default(MakeParty);
	}

	public override string ToString()
	{
		return "<MakeParty>";
	}
}

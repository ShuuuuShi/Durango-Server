using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetGrazedPets
{
	public const uint TypeCode = 29912240u;

	public static void Pack(Packer packer, GetGrazedPets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(29912240u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetGrazedPets Unpack(Unpacker unpacker)
	{
		return default(GetGrazedPets);
	}

	public override string ToString()
	{
		return "<GetGrazedPets>";
	}
}

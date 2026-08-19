using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetPetsInfo
{
	public const uint TypeCode = 14198037u;

	public static void Pack(Packer packer, GetPetsInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(14198037u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetPetsInfo Unpack(Unpacker unpacker)
	{
		return default(GetPetsInfo);
	}

	public override string ToString()
	{
		return "<GetPetsInfo>";
	}
}

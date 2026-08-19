using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetEstateLicenses
{
	public const uint TypeCode = 3820u;

	public static void Pack(Packer packer, GetEstateLicenses val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3820u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetEstateLicenses Unpack(Unpacker unpacker)
	{
		return default(GetEstateLicenses);
	}

	public override string ToString()
	{
		return "<GetEstateLicenses>";
	}
}

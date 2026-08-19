using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetClanEstateLicense
{
	public const uint TypeCode = 3697u;

	public static void Pack(Packer packer, GetClanEstateLicense val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3697u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanEstateLicense Unpack(Unpacker unpacker)
	{
		GetClanEstateLicense result = default(GetClanEstateLicense);
		return result;
	}

	public override string ToString()
	{
		return "<GetClanEstateLicense>";
	}
}

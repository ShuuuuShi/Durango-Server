using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetMails
{
	public const uint TypeCode = 2072u;

	public static void Pack(Packer packer, GetMails val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2072u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetMails Unpack(Unpacker unpacker)
	{
		return default(GetMails);
	}

	public override string ToString()
	{
		return "<GetMails>";
	}
}

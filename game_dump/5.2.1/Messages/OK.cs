using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct OK
{
	public const uint TypeCode = 1231u;

	public static void Pack(Packer packer, OK val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1231u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static OK Unpack(Unpacker unpacker)
	{
		return default(OK);
	}

	public override string ToString()
	{
		return "<OK>";
	}
}

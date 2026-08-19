using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DeregisterUser
{
	public const uint TypeCode = 1999u;

	public static void Pack(Packer packer, DeregisterUser val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1999u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static DeregisterUser Unpack(Unpacker unpacker)
	{
		DeregisterUser result = default(DeregisterUser);
		return result;
	}

	public override string ToString()
	{
		return "<DeregisterUser>";
	}
}

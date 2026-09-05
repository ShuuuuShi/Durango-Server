using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02EntreeFailed
{
	public const uint TypeCode = 222204u;

	public static void Pack(Packer packer, S02EntreeFailed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222204u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02EntreeFailed Unpack(Unpacker unpacker)
	{
		S02EntreeFailed result = default(S02EntreeFailed);
		return result;
	}

	public override string ToString()
	{
		return "<S02EntreeFailed>";
	}
}

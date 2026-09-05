using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02Leave
{
	public const uint TypeCode = 222221u;

	public static void Pack(Packer packer, S02Leave val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222221u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02Leave Unpack(Unpacker unpacker)
	{
		S02Leave result = default(S02Leave);
		return result;
	}

	public override string ToString()
	{
		return "<S02Leave>";
	}
}

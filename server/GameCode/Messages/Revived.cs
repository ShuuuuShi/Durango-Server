using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Revived
{
	public const uint TypeCode = 131u;

	public static void Pack(Packer packer, Revived val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(131u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Revived Unpack(Unpacker unpacker)
	{
		Revived result = default(Revived);
		return result;
	}

	public override string ToString()
	{
		return "<Revived>";
	}
}

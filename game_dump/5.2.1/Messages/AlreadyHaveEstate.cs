using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AlreadyHaveEstate
{
	public const uint TypeCode = 2443u;

	public static void Pack(Packer packer, AlreadyHaveEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2443u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static AlreadyHaveEstate Unpack(Unpacker unpacker)
	{
		return default(AlreadyHaveEstate);
	}

	public override string ToString()
	{
		return "<AlreadyHaveEstate>";
	}
}

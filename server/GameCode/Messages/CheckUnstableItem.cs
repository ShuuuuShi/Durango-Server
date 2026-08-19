using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CheckUnstableItem
{
	public const uint TypeCode = 1590123u;

	public static void Pack(Packer packer, CheckUnstableItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1590123u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static CheckUnstableItem Unpack(Unpacker unpacker)
	{
		CheckUnstableItem result = default(CheckUnstableItem);
		return result;
	}

	public override string ToString()
	{
		return "<CheckUnstableItem>";
	}
}

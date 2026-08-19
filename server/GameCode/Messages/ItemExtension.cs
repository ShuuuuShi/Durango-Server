using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ItemExtension
{
	public static void Pack(Packer packer, ItemExtension val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static ItemExtension Unpack(Unpacker unpacker)
	{
		ItemExtension result = default(ItemExtension);
		return result;
	}

	public override string ToString()
	{
		return "<ItemExtension>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PurchaseContent
{
	public static void Pack(Packer packer, PurchaseContent val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static PurchaseContent Unpack(Unpacker unpacker)
	{
		return default(PurchaseContent);
	}

	public override string ToString()
	{
		return "<PurchaseContent>";
	}
}

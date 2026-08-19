using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetAttachableAccessories
{
	public const uint TypeCode = 9823457u;

	public static void Pack(Packer packer, GetAttachableAccessories val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(9823457u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetAttachableAccessories Unpack(Unpacker unpacker)
	{
		GetAttachableAccessories result = default(GetAttachableAccessories);
		return result;
	}

	public override string ToString()
	{
		return "<GetAttachableAccessories>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetAppliers
{
	public const uint TypeCode = 3682u;

	public static void Pack(Packer packer, GetAppliers val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3682u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetAppliers Unpack(Unpacker unpacker)
	{
		return default(GetAppliers);
	}

	public override string ToString()
	{
		return "<GetAppliers>";
	}
}

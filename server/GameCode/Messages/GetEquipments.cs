using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetEquipments
{
	public const uint TypeCode = 2014u;

	public static void Pack(Packer packer, GetEquipments val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2014u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetEquipments Unpack(Unpacker unpacker)
	{
		GetEquipments result = default(GetEquipments);
		return result;
	}

	public override string ToString()
	{
		return "<GetEquipments>";
	}
}

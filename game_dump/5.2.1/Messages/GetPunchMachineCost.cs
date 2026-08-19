using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetPunchMachineCost
{
	public const uint TypeCode = 98241u;

	public static void Pack(Packer packer, GetPunchMachineCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(98241u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetPunchMachineCost Unpack(Unpacker unpacker)
	{
		return default(GetPunchMachineCost);
	}

	public override string ToString()
	{
		return "<GetPunchMachineCost>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DrinkWater
{
	public const uint TypeCode = 3492u;

	public static void Pack(Packer packer, DrinkWater val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3492u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static DrinkWater Unpack(Unpacker unpacker)
	{
		DrinkWater result = default(DrinkWater);
		return result;
	}

	public override string ToString()
	{
		return "<DrinkWater>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PetIsHungry
{
	public const uint TypeCode = 814u;

	public static void Pack(Packer packer, PetIsHungry val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(814u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static PetIsHungry Unpack(Unpacker unpacker)
	{
		return default(PetIsHungry);
	}

	public override string ToString()
	{
		return "<PetIsHungry>";
	}
}

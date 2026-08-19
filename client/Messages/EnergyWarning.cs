using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EnergyWarning
{
	public const uint TypeCode = 3648u;

	public static void Pack(Packer packer, EnergyWarning val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3648u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static EnergyWarning Unpack(Unpacker unpacker)
	{
		EnergyWarning result = default(EnergyWarning);
		return result;
	}

	public override string ToString()
	{
		return "<EnergyWarning>";
	}
}

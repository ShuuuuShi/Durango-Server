using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CargoWarpholeTaxToClanFund
{
	public const uint TypeCode = 3815u;

	public static void Pack(Packer packer, CargoWarpholeTaxToClanFund val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3815u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static CargoWarpholeTaxToClanFund Unpack(Unpacker unpacker)
	{
		return default(CargoWarpholeTaxToClanFund);
	}

	public override string ToString()
	{
		return "<CargoWarpholeTaxToClanFund>";
	}
}

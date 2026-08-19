using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetCargoWarpholeDefenseReward
{
	public const uint TypeCode = 3825u;

	public static void Pack(Packer packer, GetCargoWarpholeDefenseReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3825u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetCargoWarpholeDefenseReward Unpack(Unpacker unpacker)
	{
		GetCargoWarpholeDefenseReward result = default(GetCargoWarpholeDefenseReward);
		return result;
	}

	public override string ToString()
	{
		return "<GetCargoWarpholeDefenseReward>";
	}
}

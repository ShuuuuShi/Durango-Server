using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02GetSupplyRewardStatus
{
	public const uint TypeCode = 222211u;

	public static void Pack(Packer packer, S02GetSupplyRewardStatus val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222211u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02GetSupplyRewardStatus Unpack(Unpacker unpacker)
	{
		S02GetSupplyRewardStatus result = default(S02GetSupplyRewardStatus);
		return result;
	}

	public override string ToString()
	{
		return "<S02GetSupplyRewardStatus>";
	}
}

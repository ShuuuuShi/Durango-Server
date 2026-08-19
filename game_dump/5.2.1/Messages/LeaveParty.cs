using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct LeaveParty
{
	public const uint TypeCode = 20008u;

	public static void Pack(Packer packer, LeaveParty val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(20008u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static LeaveParty Unpack(Unpacker unpacker)
	{
		return default(LeaveParty);
	}

	public override string ToString()
	{
		return "<LeaveParty>";
	}
}

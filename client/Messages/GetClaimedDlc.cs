using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetClaimedDlc
{
	public const uint TypeCode = 841261u;

	public static void Pack(Packer packer, GetClaimedDlc val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(841261u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClaimedDlc Unpack(Unpacker unpacker)
	{
		GetClaimedDlc result = default(GetClaimedDlc);
		return result;
	}

	public override string ToString()
	{
		return "<GetClaimedDlc>";
	}
}

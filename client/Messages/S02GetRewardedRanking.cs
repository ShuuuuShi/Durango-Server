using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02GetRewardedRanking
{
	public const uint TypeCode = 222231u;

	public static void Pack(Packer packer, S02GetRewardedRanking val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222231u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02GetRewardedRanking Unpack(Unpacker unpacker)
	{
		S02GetRewardedRanking result = default(S02GetRewardedRanking);
		return result;
	}

	public override string ToString()
	{
		return "<S02GetRewardedRanking>";
	}
}

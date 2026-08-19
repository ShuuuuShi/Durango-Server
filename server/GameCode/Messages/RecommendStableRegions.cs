using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RecommendStableRegions
{
	public const uint TypeCode = 5792841u;

	public static void Pack(Packer packer, RecommendStableRegions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(5792841u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RecommendStableRegions Unpack(Unpacker unpacker)
	{
		RecommendStableRegions result = default(RecommendStableRegions);
		return result;
	}

	public override string ToString()
	{
		return "<RecommendStableRegions>";
	}
}

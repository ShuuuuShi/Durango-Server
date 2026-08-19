using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetStatistics
{
	public const uint TypeCode = 2039u;

	public static void Pack(Packer packer, GetStatistics val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2039u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetStatistics Unpack(Unpacker unpacker)
	{
		return default(GetStatistics);
	}

	public override string ToString()
	{
		return "<GetStatistics>";
	}
}

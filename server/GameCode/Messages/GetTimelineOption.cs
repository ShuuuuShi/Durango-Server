using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetTimelineOption
{
	public const uint TypeCode = 81234526u;

	public static void Pack(Packer packer, GetTimelineOption val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(81234526u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetTimelineOption Unpack(Unpacker unpacker)
	{
		GetTimelineOption result = default(GetTimelineOption);
		return result;
	}

	public override string ToString()
	{
		return "<GetTimelineOption>";
	}
}

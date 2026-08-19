using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MissionNotAcceptable
{
	public const uint TypeCode = 3630u;

	public static void Pack(Packer packer, MissionNotAcceptable val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3630u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static MissionNotAcceptable Unpack(Unpacker unpacker)
	{
		return default(MissionNotAcceptable);
	}

	public override string ToString()
	{
		return "<MissionNotAcceptable>";
	}
}

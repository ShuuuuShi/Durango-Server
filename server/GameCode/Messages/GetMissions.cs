using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetMissions
{
	public const uint TypeCode = 3620u;

	public static void Pack(Packer packer, GetMissions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3620u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetMissions Unpack(Unpacker unpacker)
	{
		GetMissions result = default(GetMissions);
		return result;
	}

	public override string ToString()
	{
		return "<GetMissions>";
	}
}

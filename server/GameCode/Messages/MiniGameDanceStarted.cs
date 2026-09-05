using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MiniGameDanceStarted
{
	public const uint TypeCode = 4625401u;

	public static void Pack(Packer packer, MiniGameDanceStarted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(4625401u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static MiniGameDanceStarted Unpack(Unpacker unpacker)
	{
		MiniGameDanceStarted result = default(MiniGameDanceStarted);
		return result;
	}

	public override string ToString()
	{
		return "<MiniGameDanceStarted>";
	}
}

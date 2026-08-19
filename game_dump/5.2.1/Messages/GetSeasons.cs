using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetSeasons
{
	public const uint TypeCode = 871245u;

	public static void Pack(Packer packer, GetSeasons val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(871245u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetSeasons Unpack(Unpacker unpacker)
	{
		return default(GetSeasons);
	}

	public override string ToString()
	{
		return "<GetSeasons>";
	}
}

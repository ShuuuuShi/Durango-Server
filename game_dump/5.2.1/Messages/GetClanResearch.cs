using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetClanResearch
{
	public const uint TypeCode = 5987341u;

	public static void Pack(Packer packer, GetClanResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(5987341u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanResearch Unpack(Unpacker unpacker)
	{
		return default(GetClanResearch);
	}

	public override string ToString()
	{
		return "<GetClanResearch>";
	}
}

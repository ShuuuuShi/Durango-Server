using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02GetLobbyInfo
{
	public const uint TypeCode = 222214u;

	public static void Pack(Packer packer, S02GetLobbyInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222214u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02GetLobbyInfo Unpack(Unpacker unpacker)
	{
		return default(S02GetLobbyInfo);
	}

	public override string ToString()
	{
		return "<S02GetLobbyInfo>";
	}
}

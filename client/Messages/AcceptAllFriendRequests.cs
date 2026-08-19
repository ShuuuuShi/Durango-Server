using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AcceptAllFriendRequests
{
	public const uint TypeCode = 1451221u;

	public static void Pack(Packer packer, AcceptAllFriendRequests val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1451221u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static AcceptAllFriendRequests Unpack(Unpacker unpacker)
	{
		AcceptAllFriendRequests result = default(AcceptAllFriendRequests);
		return result;
	}

	public override string ToString()
	{
		return "<AcceptAllFriendRequests>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RefuseAllFriendRequests
{
	public const uint TypeCode = 1451222u;

	public static void Pack(Packer packer, RefuseAllFriendRequests val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1451222u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RefuseAllFriendRequests Unpack(Unpacker unpacker)
	{
		return default(RefuseAllFriendRequests);
	}

	public override string ToString()
	{
		return "<RefuseAllFriendRequests>";
	}
}

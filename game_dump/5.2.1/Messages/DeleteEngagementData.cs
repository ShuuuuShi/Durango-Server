using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DeleteEngagementData
{
	public const uint TypeCode = 1444251u;

	public static void Pack(Packer packer, DeleteEngagementData val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1444251u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static DeleteEngagementData Unpack(Unpacker unpacker)
	{
		return default(DeleteEngagementData);
	}

	public override string ToString()
	{
		return "<DeleteEngagementData>";
	}
}

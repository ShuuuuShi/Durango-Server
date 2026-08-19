using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ResubscribeClanChannel
{
	public const uint TypeCode = 24u;

	public static void Pack(Packer packer, ResubscribeClanChannel val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(24u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ResubscribeClanChannel Unpack(Unpacker unpacker)
	{
		ResubscribeClanChannel result = default(ResubscribeClanChannel);
		return result;
	}

	public override string ToString()
	{
		return "<ResubscribeClanChannel>";
	}
}

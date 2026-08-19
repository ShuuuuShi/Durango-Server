using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RequestEpicWarp
{
	public const uint TypeCode = 77777u;

	public static void Pack(Packer packer, RequestEpicWarp val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(77777u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RequestEpicWarp Unpack(Unpacker unpacker)
	{
		return default(RequestEpicWarp);
	}

	public override string ToString()
	{
		return "<RequestEpicWarp>";
	}
}

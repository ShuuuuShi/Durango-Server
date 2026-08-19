using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RemoveDeathPoint
{
	public const uint TypeCode = 2034u;

	public static void Pack(Packer packer, RemoveDeathPoint val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2034u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RemoveDeathPoint Unpack(Unpacker unpacker)
	{
		return default(RemoveDeathPoint);
	}

	public override string ToString()
	{
		return "<RemoveDeathPoint>";
	}
}

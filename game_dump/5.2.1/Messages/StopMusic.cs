using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StopMusic
{
	public const uint TypeCode = 3803u;

	public static void Pack(Packer packer, StopMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3803u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static StopMusic Unpack(Unpacker unpacker)
	{
		return default(StopMusic);
	}

	public override string ToString()
	{
		return "<StopMusic>";
	}
}

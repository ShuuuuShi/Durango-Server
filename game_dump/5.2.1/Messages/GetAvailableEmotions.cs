using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetAvailableEmotions
{
	public const uint TypeCode = 9592634u;

	public static void Pack(Packer packer, GetAvailableEmotions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(9592634u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetAvailableEmotions Unpack(Unpacker unpacker)
	{
		return default(GetAvailableEmotions);
	}

	public override string ToString()
	{
		return "<GetAvailableEmotions>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetMusics
{
	public const uint TypeCode = 47852453u;

	public static void Pack(Packer packer, GetMusics val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(47852453u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetMusics Unpack(Unpacker unpacker)
	{
		return default(GetMusics);
	}

	public override string ToString()
	{
		return "<GetMusics>";
	}
}

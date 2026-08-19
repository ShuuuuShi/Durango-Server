using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetLastSearchedTime
{
	public const uint TypeCode = 906u;

	public static void Pack(Packer packer, GetLastSearchedTime val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(906u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetLastSearchedTime Unpack(Unpacker unpacker)
	{
		GetLastSearchedTime result = default(GetLastSearchedTime);
		return result;
	}

	public override string ToString()
	{
		return "<GetLastSearchedTime>";
	}
}

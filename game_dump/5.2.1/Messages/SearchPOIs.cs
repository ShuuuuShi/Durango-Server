using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SearchPOIs
{
	public const uint TypeCode = 904u;

	public static void Pack(Packer packer, SearchPOIs val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(904u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static SearchPOIs Unpack(Unpacker unpacker)
	{
		return default(SearchPOIs);
	}

	public override string ToString()
	{
		return "<SearchPOIs>";
	}
}

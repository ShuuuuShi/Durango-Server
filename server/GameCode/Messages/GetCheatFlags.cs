using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetCheatFlags
{
	public const uint TypeCode = 2088u;

	public static void Pack(Packer packer, GetCheatFlags val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2088u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetCheatFlags Unpack(Unpacker unpacker)
	{
		GetCheatFlags result = default(GetCheatFlags);
		return result;
	}

	public override string ToString()
	{
		return "<GetCheatFlags>";
	}
}

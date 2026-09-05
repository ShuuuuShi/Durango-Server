using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetActions
{
	public const uint TypeCode = 314u;

	public static void Pack(Packer packer, GetActions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(314u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetActions Unpack(Unpacker unpacker)
	{
		GetActions result = default(GetActions);
		return result;
	}

	public override string ToString()
	{
		return "<GetActions>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetTargetTitle
{
	public const uint TypeCode = 3906u;

	public static void Pack(Packer packer, GetTargetTitle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3906u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetTargetTitle Unpack(Unpacker unpacker)
	{
		GetTargetTitle result = default(GetTargetTitle);
		return result;
	}

	public override string ToString()
	{
		return "<GetTargetTitle>";
	}
}

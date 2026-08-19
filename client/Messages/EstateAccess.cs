using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EstateAccess
{
	public static void Pack(Packer packer, EstateAccess val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static EstateAccess Unpack(Unpacker unpacker)
	{
		EstateAccess result = default(EstateAccess);
		return result;
	}

	public override string ToString()
	{
		return "<EstateAccess>";
	}
}

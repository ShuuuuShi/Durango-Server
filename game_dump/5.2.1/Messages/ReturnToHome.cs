using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ReturnToHome
{
	public const uint TypeCode = 2100u;

	public static void Pack(Packer packer, ReturnToHome val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2100u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ReturnToHome Unpack(Unpacker unpacker)
	{
		return default(ReturnToHome);
	}

	public override string ToString()
	{
		return "<ReturnToHome>";
	}
}

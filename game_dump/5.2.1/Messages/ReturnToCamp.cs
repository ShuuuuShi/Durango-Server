using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ReturnToCamp
{
	public const uint TypeCode = 3462987u;

	public static void Pack(Packer packer, ReturnToCamp val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3462987u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ReturnToCamp Unpack(Unpacker unpacker)
	{
		return default(ReturnToCamp);
	}

	public override string ToString()
	{
		return "<ReturnToCamp>";
	}
}

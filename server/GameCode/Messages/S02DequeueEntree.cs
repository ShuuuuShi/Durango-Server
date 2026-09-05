using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02DequeueEntree
{
	public const uint TypeCode = 222202u;

	public static void Pack(Packer packer, S02DequeueEntree val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222202u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02DequeueEntree Unpack(Unpacker unpacker)
	{
		S02DequeueEntree result = default(S02DequeueEntree);
		return result;
	}

	public override string ToString()
	{
		return "<S02DequeueEntree>";
	}
}

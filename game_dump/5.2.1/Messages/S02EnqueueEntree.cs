using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct S02EnqueueEntree
{
	public const uint TypeCode = 222201u;

	public static void Pack(Packer packer, S02EnqueueEntree val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(222201u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static S02EnqueueEntree Unpack(Unpacker unpacker)
	{
		return default(S02EnqueueEntree);
	}

	public override string ToString()
	{
		return "<S02EnqueueEntree>";
	}
}

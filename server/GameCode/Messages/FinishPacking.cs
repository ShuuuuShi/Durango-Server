using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FinishPacking
{
	public const uint TypeCode = 3771u;

	public static void Pack(Packer packer, FinishPacking val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3771u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static FinishPacking Unpack(Unpacker unpacker)
	{
		FinishPacking result = default(FinishPacking);
		return result;
	}

	public override string ToString()
	{
		return "<FinishPacking>";
	}
}

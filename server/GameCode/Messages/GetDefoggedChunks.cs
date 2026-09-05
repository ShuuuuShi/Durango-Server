using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetDefoggedChunks
{
	public const uint TypeCode = 204u;

	public static void Pack(Packer packer, GetDefoggedChunks val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(204u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetDefoggedChunks Unpack(Unpacker unpacker)
	{
		GetDefoggedChunks result = default(GetDefoggedChunks);
		return result;
	}

	public override string ToString()
	{
		return "<GetDefoggedChunks>";
	}
}

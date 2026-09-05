using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CancelTargetTitle
{
	public const uint TypeCode = 3901u;

	public static void Pack(Packer packer, CancelTargetTitle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3901u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static CancelTargetTitle Unpack(Unpacker unpacker)
	{
		CancelTargetTitle result = default(CancelTargetTitle);
		return result;
	}

	public override string ToString()
	{
		return "<CancelTargetTitle>";
	}
}

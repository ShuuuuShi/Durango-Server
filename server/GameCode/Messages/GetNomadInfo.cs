using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetNomadInfo
{
	public const uint TypeCode = 100000u;

	public static void Pack(Packer packer, GetNomadInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(100000u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetNomadInfo Unpack(Unpacker unpacker)
	{
		GetNomadInfo result = default(GetNomadInfo);
		return result;
	}

	public override string ToString()
	{
		return "<GetNomadInfo>";
	}
}

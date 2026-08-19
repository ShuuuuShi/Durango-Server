using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetSocialOptions
{
	public const uint TypeCode = 24000u;

	public static void Pack(Packer packer, GetSocialOptions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(24000u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetSocialOptions Unpack(Unpacker unpacker)
	{
		return default(GetSocialOptions);
	}

	public override string ToString()
	{
		return "<GetSocialOptions>";
	}
}

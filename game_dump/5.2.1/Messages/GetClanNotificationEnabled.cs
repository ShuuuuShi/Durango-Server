using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetClanNotificationEnabled
{
	public const uint TypeCode = 4027u;

	public static void Pack(Packer packer, GetClanNotificationEnabled val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(4027u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanNotificationEnabled Unpack(Unpacker unpacker)
	{
		return default(GetClanNotificationEnabled);
	}

	public override string ToString()
	{
		return "<GetClanNotificationEnabled>";
	}
}

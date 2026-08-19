using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ClanRewardsUpdated
{
	public const uint TypeCode = 3705u;

	public static void Pack(Packer packer, ClanRewardsUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3705u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ClanRewardsUpdated Unpack(Unpacker unpacker)
	{
		return default(ClanRewardsUpdated);
	}

	public override string ToString()
	{
		return "<ClanRewardsUpdated>";
	}
}

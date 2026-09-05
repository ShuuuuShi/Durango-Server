using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ClanStatusEffectsUpdated
{
	public const uint TypeCode = 3703u;

	public static void Pack(Packer packer, ClanStatusEffectsUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3703u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ClanStatusEffectsUpdated Unpack(Unpacker unpacker)
	{
		ClanStatusEffectsUpdated result = default(ClanStatusEffectsUpdated);
		return result;
	}

	public override string ToString()
	{
		return "<ClanStatusEffectsUpdated>";
	}
}

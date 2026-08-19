using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetClanCreationCosts
{
	public const uint TypeCode = 3667u;

	public static void Pack(Packer packer, GetClanCreationCosts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3667u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanCreationCosts Unpack(Unpacker unpacker)
	{
		return default(GetClanCreationCosts);
	}

	public override string ToString()
	{
		return "<GetClanCreationCosts>";
	}
}

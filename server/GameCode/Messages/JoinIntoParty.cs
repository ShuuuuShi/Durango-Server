using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct JoinIntoParty
{
	public const uint TypeCode = 20006u;

	public static void Pack(Packer packer, JoinIntoParty val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(20006u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static JoinIntoParty Unpack(Unpacker unpacker)
	{
		JoinIntoParty result = default(JoinIntoParty);
		return result;
	}

	public override string ToString()
	{
		return "<JoinIntoParty>";
	}
}

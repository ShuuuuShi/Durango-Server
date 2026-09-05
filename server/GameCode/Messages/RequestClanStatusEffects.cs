using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RequestClanStatusEffects
{
	public const uint TypeCode = 3704u;

	public static void Pack(Packer packer, RequestClanStatusEffects val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3704u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RequestClanStatusEffects Unpack(Unpacker unpacker)
	{
		RequestClanStatusEffects result = default(RequestClanStatusEffects);
		return result;
	}

	public override string ToString()
	{
		return "<RequestClanStatusEffects>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ReissueArchipelagoTodos
{
	public const uint TypeCode = 240005u;

	public static void Pack(Packer packer, ReissueArchipelagoTodos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(240005u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static ReissueArchipelagoTodos Unpack(Unpacker unpacker)
	{
		ReissueArchipelagoTodos result = default(ReissueArchipelagoTodos);
		return result;
	}

	public override string ToString()
	{
		return "<ReissueArchipelagoTodos>";
	}
}

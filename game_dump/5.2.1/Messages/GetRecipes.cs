using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetRecipes
{
	public const uint TypeCode = 2011u;

	public static void Pack(Packer packer, GetRecipes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2011u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetRecipes Unpack(Unpacker unpacker)
	{
		return default(GetRecipes);
	}

	public override string ToString()
	{
		return "<GetRecipes>";
	}
}

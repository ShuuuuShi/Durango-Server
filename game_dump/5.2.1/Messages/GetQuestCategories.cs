using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetQuestCategories
{
	public const uint TypeCode = 237901u;

	public static void Pack(Packer packer, GetQuestCategories val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(237901u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetQuestCategories Unpack(Unpacker unpacker)
	{
		return default(GetQuestCategories);
	}

	public override string ToString()
	{
		return "<GetQuestCategories>";
	}
}

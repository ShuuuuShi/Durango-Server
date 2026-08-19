using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetSkills
{
	public const uint TypeCode = 2047u;

	public static void Pack(Packer packer, GetSkills val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2047u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetSkills Unpack(Unpacker unpacker)
	{
		return default(GetSkills);
	}

	public override string ToString()
	{
		return "<GetSkills>";
	}
}

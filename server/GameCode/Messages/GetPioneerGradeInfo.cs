using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetPioneerGradeInfo
{
	public const uint TypeCode = 812234574u;

	public static void Pack(Packer packer, GetPioneerGradeInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(812234574u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetPioneerGradeInfo Unpack(Unpacker unpacker)
	{
		GetPioneerGradeInfo result = default(GetPioneerGradeInfo);
		return result;
	}

	public override string ToString()
	{
		return "<GetPioneerGradeInfo>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetAdvisorTargets
{
	public const uint TypeCode = 3708u;

	public static void Pack(Packer packer, GetAdvisorTargets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3708u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetAdvisorTargets Unpack(Unpacker unpacker)
	{
		GetAdvisorTargets result = default(GetAdvisorTargets);
		return result;
	}

	public override string ToString()
	{
		return "<GetAdvisorTargets>";
	}
}

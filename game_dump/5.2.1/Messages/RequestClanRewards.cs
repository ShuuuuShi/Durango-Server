using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RequestClanRewards
{
	public const uint TypeCode = 3706u;

	public static void Pack(Packer packer, RequestClanRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3706u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static RequestClanRewards Unpack(Unpacker unpacker)
	{
		return default(RequestClanRewards);
	}

	public override string ToString()
	{
		return "<RequestClanRewards>";
	}
}

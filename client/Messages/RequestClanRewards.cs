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
		RequestClanRewards result = default(RequestClanRewards);
		return result;
	}

	public override string ToString()
	{
		return "<RequestClanRewards>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TodoGoal
{
	public static void Pack(Packer packer, TodoGoal val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static TodoGoal Unpack(Unpacker unpacker)
	{
		return default(TodoGoal);
	}

	public override string ToString()
	{
		return "<TodoGoal>";
	}
}

using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Todo
{
	public static void Pack(Packer packer, Todo val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static Todo Unpack(Unpacker unpacker)
	{
		return default(Todo);
	}

	public override string ToString()
	{
		return "<Todo>";
	}
}

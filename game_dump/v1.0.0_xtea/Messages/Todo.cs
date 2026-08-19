using MsgPack;

namespace Messages;

public struct Todo
{
	public static void Pack(Packer packer, Todo val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static Todo Unpack(Unpacker unpacker)
	{
		Todo result = default(Todo);
		return result;
	}

	public override string ToString()
	{
		return "<Todo>";
	}
}

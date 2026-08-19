using MsgPack;

namespace Messages;

public struct TodoGoal
{
	public static void Pack(Packer packer, TodoGoal val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static TodoGoal Unpack(Unpacker unpacker)
	{
		TodoGoal result = default(TodoGoal);
		return result;
	}

	public override string ToString()
	{
		return "<TodoGoal>";
	}
}

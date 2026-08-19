using MsgPack;

namespace Messages;

public struct GetMemos
{
	public const uint TypeCode = 2439u;

	public static void Pack(Packer packer, GetMemos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2439u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetMemos Unpack(Unpacker unpacker)
	{
		GetMemos result = default(GetMemos);
		return result;
	}

	public override string ToString()
	{
		return "<GetMemos>";
	}
}

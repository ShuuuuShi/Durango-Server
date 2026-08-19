using MsgPack;

namespace Messages;

public struct Null
{
	public const uint TypeCode = 255u;

	public static void Pack(Packer packer, Null val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(255u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Null Unpack(Unpacker unpacker)
	{
		Null result = default(Null);
		return result;
	}

	public override string ToString()
	{
		return "<Null>";
	}
}

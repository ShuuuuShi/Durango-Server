using MsgPack;

namespace Messages;

public struct Inspected
{
	public const uint TypeCode = 3604u;

	public static void Pack(Packer packer, Inspected val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3604u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Inspected Unpack(Unpacker unpacker)
	{
		Inspected result = default(Inspected);
		return result;
	}

	public override string ToString()
	{
		return "<Inspected>";
	}
}

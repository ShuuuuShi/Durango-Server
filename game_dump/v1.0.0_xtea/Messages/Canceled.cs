using MsgPack;

namespace Messages;

public struct Canceled
{
	public const uint TypeCode = 2038u;

	public static void Pack(Packer packer, Canceled val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2038u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Canceled Unpack(Unpacker unpacker)
	{
		Canceled result = default(Canceled);
		return result;
	}

	public override string ToString()
	{
		return "<Canceled>";
	}
}

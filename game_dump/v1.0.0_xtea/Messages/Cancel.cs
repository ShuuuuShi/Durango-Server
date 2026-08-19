using MsgPack;

namespace Messages;

public struct Cancel
{
	public const uint TypeCode = 2036u;

	public static void Pack(Packer packer, Cancel val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2036u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Cancel Unpack(Unpacker unpacker)
	{
		Cancel result = default(Cancel);
		return result;
	}

	public override string ToString()
	{
		return "<Cancel>";
	}
}

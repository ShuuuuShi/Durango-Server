using MsgPack;

namespace Messages;

public struct OK
{
	public const uint TypeCode = 1231u;

	public static void Pack(Packer packer, OK val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(1231u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static OK Unpack(Unpacker unpacker)
	{
		OK result = default(OK);
		return result;
	}

	public override string ToString()
	{
		return "<OK>";
	}
}

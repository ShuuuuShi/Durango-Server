using MsgPack;

namespace Messages;

public struct WashBody
{
	public const uint TypeCode = 3494u;

	public static void Pack(Packer packer, WashBody val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3494u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static WashBody Unpack(Unpacker unpacker)
	{
		WashBody result = default(WashBody);
		return result;
	}

	public override string ToString()
	{
		return "<WashBody>";
	}
}

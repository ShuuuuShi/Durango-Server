using MsgPack;

namespace Messages;

public struct Emigrated
{
	public const uint TypeCode = 2099u;

	public static void Pack(Packer packer, Emigrated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2099u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static Emigrated Unpack(Unpacker unpacker)
	{
		Emigrated result = default(Emigrated);
		return result;
	}

	public override string ToString()
	{
		return "<Emigrated>";
	}
}

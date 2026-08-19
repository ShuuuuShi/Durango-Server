using MsgPack;

namespace Messages;

public struct GiveupEstate
{
	public const uint TypeCode = 2426u;

	public static void Pack(Packer packer, GiveupEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2426u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GiveupEstate Unpack(Unpacker unpacker)
	{
		GiveupEstate result = default(GiveupEstate);
		return result;
	}

	public override string ToString()
	{
		return "<GiveupEstate>";
	}
}

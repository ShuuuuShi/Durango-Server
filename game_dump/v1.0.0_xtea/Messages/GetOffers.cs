using MsgPack;

namespace Messages;

public struct GetOffers
{
	public const uint TypeCode = 3498u;

	public static void Pack(Packer packer, GetOffers val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3498u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetOffers Unpack(Unpacker unpacker)
	{
		GetOffers result = default(GetOffers);
		return result;
	}

	public override string ToString()
	{
		return "<GetOffers>";
	}
}

using MsgPack;

namespace Messages;

public struct TerritoryUpdated
{
	public const uint TypeCode = 3676u;

	public static void Pack(Packer packer, TerritoryUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3676u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static TerritoryUpdated Unpack(Unpacker unpacker)
	{
		TerritoryUpdated result = default(TerritoryUpdated);
		return result;
	}

	public override string ToString()
	{
		return "<TerritoryUpdated>";
	}
}

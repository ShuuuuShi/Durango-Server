using MsgPack;

namespace Messages;

public struct GetWarCosts
{
	public const uint TypeCode = 3671u;

	public static void Pack(Packer packer, GetWarCosts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3671u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetWarCosts Unpack(Unpacker unpacker)
	{
		GetWarCosts result = default(GetWarCosts);
		return result;
	}

	public override string ToString()
	{
		return "<GetWarCosts>";
	}
}

using MsgPack;

namespace Messages;

public struct GetClanTerritoryCosts
{
	public const uint TypeCode = 3677u;

	public static void Pack(Packer packer, GetClanTerritoryCosts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(3677u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetClanTerritoryCosts Unpack(Unpacker unpacker)
	{
		GetClanTerritoryCosts result = default(GetClanTerritoryCosts);
		return result;
	}

	public override string ToString()
	{
		return "<GetClanTerritoryCosts>";
	}
}

using MsgPack;

namespace Messages;

public struct POIConquered
{
	public const uint TypeCode = 3665u;

	public Point2 Tile;

	public ulong ClanId;

	public static void Pack(Packer packer, POIConquered val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3665u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.ClanId);
	}

	public static POIConquered Unpack(Unpacker unpacker)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		POIConquered result = default(POIConquered);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.ClanId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<POIConquered Tile={Tile} ClanId={ClanId}>";
	}
}

using MsgPack;

namespace Messages;

public struct RegionTile
{
	public Region Region;

	public Point2 Tile;

	public static void Pack(Packer packer, RegionTile val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		Region.Pack(packer, val.Region);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static RegionTile Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RegionTile result = default(RegionTile);
		result.Region = Region.Unpack(unpacker);
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		return result;
	}

	public override string ToString()
	{
		return $"<RegionTile Region={Region} Tile={Tile}>";
	}
}

using MsgPack;

namespace Messages;

public struct PlantSeed
{
	public const uint TypeCode = 3806u;

	public string EntityId;

	public Point2 Tile;

	public string SeedItemId;

	public static void Pack(Packer packer, PlantSeed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3806u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.SeedItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SeedItemId);
		}
	}

	public static PlantSeed Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlantSeed result = default(PlantSeed);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.SeedItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlantSeed EntityId={EntityId} Tile={Tile} SeedItemId={SeedItemId}>";
	}
}

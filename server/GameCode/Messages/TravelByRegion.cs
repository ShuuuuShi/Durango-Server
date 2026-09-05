using MsgPack;

namespace Messages;

public struct TravelByRegion
{
	public const uint TypeCode = 2029u;

	public string EntityId;

	public Point2 Tile;

	public string RegionId;

	public string PartierId;

	public static void Pack(Packer packer, TravelByRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2029u);
		}
		else
		{
			packer.PackArrayHeader(4);
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
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		if (val.PartierId == null)
		{
			packer.PackNull();
		}
		else if (val.PartierId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PartierId);
		}
	}

	public static TravelByRegion Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TravelByRegion result = default(TravelByRegion);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PartierId = null;
		}
		else
		{
			string partierId = unpacker.LastReadData.AsString();
			result.PartierId = partierId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TravelByRegion EntityId={EntityId} Tile={Tile} RegionId={RegionId} PartierId={PartierId}>";
	}
}

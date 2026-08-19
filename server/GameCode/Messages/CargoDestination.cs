using MsgPack;

namespace Messages;

public struct CargoDestination
{
	public string RegionId;

	public Point2 Tile;

	public string EntityId;

	public static void Pack(Packer packer, CargoDestination val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static CargoDestination Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CargoDestination result = default(CargoDestination);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<CargoDestination RegionId={RegionId} Tile={Tile} EntityId={EntityId}>";
	}
}

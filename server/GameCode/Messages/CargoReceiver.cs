using MsgPack;

namespace Messages;

public struct CargoReceiver
{
	public const uint TypeCode = 103495712u;

	public Region Region;

	public Point2 Tile;

	public string EntityId;

	public int UsingSize;

	public int MaxSize;

	public static void Pack(Packer packer, CargoReceiver val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(103495712u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		Region.Pack(packer, val.Region);
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
		packer.Pack(val.UsingSize);
		packer.Pack(val.MaxSize);
	}

	public static CargoReceiver Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CargoReceiver result = default(CargoReceiver);
		result.Region = Region.Unpack(unpacker);
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.UsingSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MaxSize = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<CargoReceiver Region={Region} Tile={Tile} EntityId={EntityId} UsingSize={UsingSize} MaxSize={MaxSize}>";
	}
}

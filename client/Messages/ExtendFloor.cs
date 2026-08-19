using MsgPack;

namespace Messages;

public struct ExtendFloor
{
	public const uint TypeCode = 25565u;

	public string EntityId;

	public Point2 Tile;

	public bool WithRoof;

	public static void Pack(Packer packer, ExtendFloor val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(25565u);
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
		packer.Pack(val.WithRoof);
	}

	public static ExtendFloor Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ExtendFloor result = default(ExtendFloor);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.WithRoof = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ExtendFloor EntityId={EntityId} Tile={Tile} WithRoof={WithRoof}>";
	}
}

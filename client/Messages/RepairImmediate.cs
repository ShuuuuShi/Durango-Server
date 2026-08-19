using MsgPack;

namespace Messages;

public struct RepairImmediate
{
	public const uint TypeCode = 2056u;

	public string EntityId;

	public Point2 Tile;

	public Cost Cost;

	public static void Pack(Packer packer, RepairImmediate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2056u);
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
		Cost.Pack(packer, val.Cost);
	}

	public static RepairImmediate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RepairImmediate result = default(RepairImmediate);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Cost = Cost.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RepairImmediate EntityId={EntityId} Tile={Tile} Cost={Cost}>";
	}
}

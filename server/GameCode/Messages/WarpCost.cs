using MsgPack;

namespace Messages;

public struct WarpCost
{
	public Point2 Tile;

	public long Cost;

	public bool Prohibited;

	public static void Pack(Packer packer, WarpCost val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.Cost);
		packer.Pack(val.Prohibited);
	}

	public static WarpCost Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadUInt16(out var result);
		WarpCost result2 = default(WarpCost);
		result2.Tile.x = result;
		unpacker.ReadUInt16(out result);
		result2.Tile.y = result;
		unpacker.Read();
		result2.Cost = unpacker.LastReadData.AsInt64();
		unpacker.Read();
		result2.Prohibited = unpacker.LastReadData.AsBoolean();
		return result2;
	}

	public override string ToString()
	{
		return $"<WarpCost Tile={Tile} Cost={Cost} Prohibited={Prohibited}>";
	}
}

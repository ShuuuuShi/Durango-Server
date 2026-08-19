using MsgPack;

namespace Messages;

public struct WarpCost
{
	public Point2 Tile;

	public int Cost;

	public static void Pack(Packer packer, WarpCost val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.Cost);
	}

	public static WarpCost Unpack(Unpacker unpacker)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		WarpCost result = default(WarpCost);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Cost = ((MessagePackObject)(ref lastReadData)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<WarpCost Tile={Tile} Cost={Cost}>";
	}
}

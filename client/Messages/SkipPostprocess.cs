using MsgPack;

namespace Messages;

public struct SkipPostprocess
{
	public const uint TypeCode = 2450u;

	public string EntityId;

	public Point2 Tile;

	public long Cost;

	public static void Pack(Packer packer, SkipPostprocess val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2450u);
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
		packer.Pack(val.Cost);
	}

	public static SkipPostprocess Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkipPostprocess result = default(SkipPostprocess);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Cost = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<SkipPostprocess EntityId={EntityId} Tile={Tile} Cost={Cost}>";
	}
}

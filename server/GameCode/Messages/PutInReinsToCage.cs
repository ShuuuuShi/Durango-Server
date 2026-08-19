using MsgPack;

namespace Messages;

public struct PutInReinsToCage
{
	public const uint TypeCode = 694351u;

	public string EntityId;

	public Point2 Tile;

	public string ItemId;

	public static void Pack(Packer packer, PutInReinsToCage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(694351u);
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
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
	}

	public static PutInReinsToCage Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PutInReinsToCage result = default(PutInReinsToCage);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.ItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PutInReinsToCage EntityId={EntityId} Tile={Tile} ItemId={ItemId}>";
	}
}

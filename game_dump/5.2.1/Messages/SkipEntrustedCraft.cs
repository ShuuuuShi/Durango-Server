using MsgPack;

namespace Messages;

public struct SkipEntrustedCraft
{
	public const uint TypeCode = 7498153u;

	public string EntityId;

	public Point2 Tile;

	public string CraftingId;

	public static void Pack(Packer packer, SkipEntrustedCraft val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(7498153u);
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
		if (val.CraftingId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CraftingId);
		}
	}

	public static SkipEntrustedCraft Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkipEntrustedCraft result = default(SkipEntrustedCraft);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.CraftingId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SkipEntrustedCraft EntityId={EntityId} Tile={Tile} CraftingId={CraftingId}>";
	}
}

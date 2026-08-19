using MsgPack;

namespace Messages;

public struct RenameWarehouseSection
{
	public const uint TypeCode = 3696u;

	public string EntityId;

	public Point2 Tile;

	public string SectionName;

	public string NewName;

	public static void Pack(Packer packer, RenameWarehouseSection val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3696u);
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
		if (val.SectionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SectionName);
		}
		if (val.NewName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.NewName);
		}
	}

	public static RenameWarehouseSection Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RenameWarehouseSection result = default(RenameWarehouseSection);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.SectionName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.NewName = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RenameWarehouseSection EntityId={EntityId} Tile={Tile} SectionName={SectionName} NewName={NewName}>";
	}
}

using MsgPack;

namespace Messages;

public struct RemoveSection
{
	public const uint TypeCode = 3687u;

	public string EntityId;

	public Point2 Tile;

	public string SectionName;

	public static void Pack(Packer packer, RemoveSection val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3687u);
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
		if (val.SectionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SectionName);
		}
	}

	public static RemoveSection Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RemoveSection result = default(RemoveSection);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.SectionName = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RemoveSection EntityId={EntityId} Tile={Tile} SectionName={SectionName}>";
	}
}

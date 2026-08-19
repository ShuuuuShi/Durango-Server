using MsgPack;

namespace Messages;

public struct MakeSection
{
	public const uint TypeCode = 3686u;

	public string EntityId;

	public Point2 Tile;

	public string SectionName;

	public static void Pack(Packer packer, MakeSection val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3686u);
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

	public static MakeSection Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MakeSection result = default(MakeSection);
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
		return $"<MakeSection EntityId={EntityId} Tile={Tile} SectionName={SectionName}>";
	}
}

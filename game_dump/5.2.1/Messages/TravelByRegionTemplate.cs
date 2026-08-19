using MsgPack;

namespace Messages;

public struct TravelByRegionTemplate
{
	public const uint TypeCode = 2031u;

	public string EntityId;

	public Point2 Tile;

	public string RegionTemplateId;

	public static void Pack(Packer packer, TravelByRegionTemplate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2031u);
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
		if (val.RegionTemplateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionTemplateId);
		}
	}

	public static TravelByRegionTemplate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TravelByRegionTemplate result = default(TravelByRegionTemplate);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.RegionTemplateId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<TravelByRegionTemplate EntityId={EntityId} Tile={Tile} RegionTemplateId={RegionTemplateId}>";
	}
}

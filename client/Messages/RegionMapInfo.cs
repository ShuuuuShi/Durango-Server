using MsgPack;

namespace Messages;

public struct RegionMapInfo
{
	public const uint TypeCode = 206u;

	public string RegionId;

	public string TerrainId;

	public Point2 TileCount;

	public DefoggedChunks DefoggedChunks;

	public static void Pack(Packer packer, RegionMapInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(206u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		if (val.TerrainId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TerrainId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.TileCount.x);
		packer.Pack((ushort)val.TileCount.y);
		DefoggedChunks.Pack(packer, val.DefoggedChunks);
	}

	public static RegionMapInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RegionMapInfo result = default(RegionMapInfo);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TerrainId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.TileCount.x = result2;
		unpacker.ReadUInt16(out result2);
		result.TileCount.y = result2;
		unpacker.Read();
		result.DefoggedChunks = DefoggedChunks.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RegionMapInfo RegionId={RegionId} TerrainId={TerrainId} TileCount={TileCount} DefoggedChunks={DefoggedChunks}>";
	}
}

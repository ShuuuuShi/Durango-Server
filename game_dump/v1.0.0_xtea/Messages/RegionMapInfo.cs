using MsgPack;

namespace Messages;

public struct RegionMapInfo
{
	public const uint TypeCode = 206u;

	public ulong RegionId;

	public Point2 TileCount;

	public DefoggedChunks DefoggedChunks;

	public static void Pack(Packer packer, RegionMapInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(206u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.RegionId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.TileCount.x);
		packer.Pack((ushort)val.TileCount.y);
		DefoggedChunks.Pack(packer, val.DefoggedChunks);
	}

	public static RegionMapInfo Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RegionMapInfo result = default(RegionMapInfo);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.TileCount.x = num;
		unpacker.ReadUInt16(ref num);
		result.TileCount.y = num;
		unpacker.Read();
		result.DefoggedChunks = DefoggedChunks.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RegionMapInfo RegionId={RegionId} TileCount={TileCount} DefoggedChunks={DefoggedChunks}>";
	}
}

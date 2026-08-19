using MsgPack;

namespace Messages;

public struct SprinkledInfo
{
	public const uint TypeCode = 37122u;

	public Point2[] FertilizedTiles;

	public static void Pack(Packer packer, SprinkledInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(37122u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.FertilizedTiles == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.FertilizedTiles.Length);
		for (int i = 0; i < val.FertilizedTiles.Length; i++)
		{
			packer.PackArrayHeader(2);
			packer.Pack((ushort)val.FertilizedTiles[i].x);
			packer.Pack((ushort)val.FertilizedTiles[i].y);
		}
	}

	public static SprinkledInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SprinkledInfo result = default(SprinkledInfo);
		result.FertilizedTiles = new Point2[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadUInt16(out var result2);
			result.FertilizedTiles[i].x = result2;
			unpacker.ReadUInt16(out result2);
			result.FertilizedTiles[i].y = result2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SprinkledInfo FertilizedTiles={FertilizedTiles}>";
	}
}

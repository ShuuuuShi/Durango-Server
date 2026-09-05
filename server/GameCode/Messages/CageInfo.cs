using MsgPack;

namespace Messages;

public struct CageInfo
{
	public const uint TypeCode = 950843u;

	public string RegionId;

	public string RegionName;

	public Point2 Tile;

	public static void Pack(Packer packer, CageInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(950843u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.PackString(val.RegionName);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static CageInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CageInfo result = default(CageInfo);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RegionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<CageInfo RegionId={RegionId} RegionName={RegionName} Tile={Tile}>";
	}
}

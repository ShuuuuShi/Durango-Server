using MsgPack;

namespace Messages;

public struct RadioPin
{
	public const uint TypeCode = 2605u;

	public string RegionId;

	public string RegionName;

	public Point2 Tile;

	public static void Pack(Packer packer, RadioPin val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2605u);
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
		if (val.RegionName == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.RegionName);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static RadioPin Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioPin result = default(RadioPin);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RegionName = null;
		}
		else
		{
			string regionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.RegionName = regionName;
		}
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<RadioPin RegionId={RegionId} RegionName={RegionName} Tile={Tile}>";
	}
}

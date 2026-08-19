using MsgPack;

namespace Messages;

public struct Points
{
	public const uint TypeCode = 2033u;

	public EntityTile? HomePoint;

	public RegionTile ReturningPoint;

	public RegionTile? DeathPoint;

	public RegionTile? LastReturnPoint;

	public RegionTile? CampPoint;

	public static void Pack(Packer packer, Points val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2033u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (!val.HomePoint.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EntityTile.Pack(packer, val.HomePoint.Value);
		}
		RegionTile.Pack(packer, val.ReturningPoint);
		if (!val.DeathPoint.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RegionTile.Pack(packer, val.DeathPoint.Value);
		}
		if (!val.LastReturnPoint.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RegionTile.Pack(packer, val.LastReturnPoint.Value);
		}
		if (!val.CampPoint.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			RegionTile.Pack(packer, val.CampPoint.Value);
		}
	}

	public static Points Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Points result = default(Points);
		if (unpacker.LastReadData.IsNil)
		{
			result.HomePoint = null;
		}
		else
		{
			EntityTile value = EntityTile.Unpack(unpacker);
			result.HomePoint = value;
		}
		unpacker.Read();
		result.ReturningPoint = RegionTile.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.DeathPoint = null;
		}
		else
		{
			RegionTile value2 = RegionTile.Unpack(unpacker);
			result.DeathPoint = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.LastReturnPoint = null;
		}
		else
		{
			RegionTile value3 = RegionTile.Unpack(unpacker);
			result.LastReturnPoint = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CampPoint = null;
		}
		else
		{
			RegionTile value4 = RegionTile.Unpack(unpacker);
			result.CampPoint = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Points HomePoint={HomePoint} ReturningPoint={ReturningPoint} DeathPoint={DeathPoint} LastReturnPoint={LastReturnPoint} CampPoint={CampPoint}>";
	}
}

using MsgPack;

namespace Messages;

public struct Points
{
	public const uint TypeCode = 2033u;

	public EntityTile? BasePoint;

	public EntityTile? HomePoint;

	public RegionTile ReturningPoint;

	public RegionTile? DeathPoint;

	public RegionTile? LastReturnPoint;

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
		if (!val.BasePoint.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EntityTile.Pack(packer, val.BasePoint.Value);
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
	}

	public static Points Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Points result = default(Points);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.BasePoint = null;
		}
		else
		{
			EntityTile value = EntityTile.Unpack(unpacker);
			result.BasePoint = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.HomePoint = null;
		}
		else
		{
			EntityTile value2 = EntityTile.Unpack(unpacker);
			result.HomePoint = value2;
		}
		unpacker.Read();
		result.ReturningPoint = RegionTile.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.DeathPoint = null;
		}
		else
		{
			RegionTile value3 = RegionTile.Unpack(unpacker);
			result.DeathPoint = value3;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.LastReturnPoint = null;
		}
		else
		{
			RegionTile value4 = RegionTile.Unpack(unpacker);
			result.LastReturnPoint = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Points BasePoint={BasePoint} HomePoint={HomePoint} ReturningPoint={ReturningPoint} DeathPoint={DeathPoint} LastReturnPoint={LastReturnPoint}>";
	}
}

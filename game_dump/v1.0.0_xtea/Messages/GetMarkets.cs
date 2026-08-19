using MsgPack;

namespace Messages;

public struct GetMarkets
{
	public const uint TypeCode = 5004u;

	public ulong? SellerId;

	public ulong? RegionId;

	public int? Skip;

	public int? Limit;

	public static void Pack(Packer packer, GetMarkets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(5004u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (!val.SellerId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.SellerId.Value);
		}
		if (!val.RegionId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.RegionId.Value);
		}
		if (!val.Skip.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Skip.Value);
		}
		if (!val.Limit.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Limit.Value);
		}
	}

	public static GetMarkets Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetMarkets result = default(GetMarkets);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.SellerId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
			result.SellerId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.RegionId = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			ulong value2 = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
			result.RegionId = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData5)).IsNil)
		{
			result.Skip = null;
		}
		else
		{
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int value3 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			result.Skip = value3;
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData7)).IsNil)
		{
			result.Limit = null;
		}
		else
		{
			MessagePackObject lastReadData8 = unpacker.LastReadData;
			int value4 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
			result.Limit = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetMarkets SellerId={SellerId} RegionId={RegionId} Skip={Skip} Limit={Limit}>";
	}
}
